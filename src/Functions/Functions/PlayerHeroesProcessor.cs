using Ingweland.Fog.Application.Server.Interfaces.Hoh;
using Ingweland.Fog.Application.Server.Providers;
using Ingweland.Fog.Application.Server.Services.Hoh.Abstractions;
using Ingweland.Fog.Functions.Services;
using Ingweland.Fog.Inn.Models.Hoh;
using Ingweland.Fog.InnSdk.Hoh.Providers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Functions.Functions;

public class PlayerHeroesProcessor(
    IGameWorldsProvider gameWorldsProvider,
    IInGameRawDataTableRepository inGameRawDataTableRepository,
    IInGameDataParsingService inGameDataParsingService,
    InGameRawDataTablePartitionKeyProvider inGameRawDataTablePartitionKeyProvider,
    IPlayerHeroesService playerHeroesService,
    ILogger<PlayerHeroesProcessor> logger,
    DatabaseWarmUpService databaseWarmUpService) : FunctionBase(gameWorldsProvider, inGameRawDataTableRepository,
    inGameDataParsingService, inGameRawDataTablePartitionKeyProvider, logger)
{
    [Function(nameof(PlayerHeroesProcessor))]
    public async Task<bool> Run([ActivityTrigger] int dataPage)
    {
        logger.LogInformation("{activity} started.", nameof(PlayerHeroesProcessor));
        await databaseWarmUpService.WarmUpDatabaseIfRequiredAsync();

        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        logger.LogInformation("PlayerHeroesProcessor started for date {date}", date);
        foreach (var gameWorld in GameWorldsProvider.GetGameWorlds())
        {
            logger.LogInformation("Processing game world {gameWorldId}", gameWorld.Id);

            var allUnits = new Dictionary<int, HashSet<string>>();
            var allHeroes = new Dictionary<int, HashSet<string>>();

            var data = await GetDataAsync(InGameRawDataTablePartitionKeyProvider.HeroesWakeup(gameWorld.Id, date),
                dataPage);
            foreach (var valueTuple in data)
            {
                foreach (var inGameEvent in valueTuple.CommunicationDto.InGameEvents)
                {
                    var state = inGameEvent.GetState<PvpEventStateDTO>("pvp_event.PvP", "pvp_event.EliteArena");
                    if (state != null)
                    {
                        var units = state.BattleLocations.ToDictionary(x => x.EnemyId,
                            x => x.Stages.SelectMany(y => y.Enemies.Select(h => h.Hero.UnitId).ToHashSet()));

                        foreach (var kvp in units)
                        {
                            Merge(allUnits, kvp.Key, kvp.Value);
                        }
                    }
                }

                var heroes = valueTuple.CommunicationDto.WoaHeroRosters
                    .GroupBy(x => x.PlayerId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.SelectMany(x => x.Heroes).Select(y => y.HeroDefinitionId).ToHashSet());
                foreach (var kvp in heroes)
                {
                    Merge(allHeroes, kvp.Key, kvp.Value);
                }
            }

            var startupData =
                await GetDataAsync(InGameRawDataTablePartitionKeyProvider.HeroesStartup(gameWorld.Id, date), dataPage);
            foreach (var valueTuple in startupData)
            {
                try
                {
                    Merge(allHeroes, (int) valueTuple.CommunicationDto.Player.Id,
                        valueTuple.CommunicationDto.HeroPush.Unlocked.Select(x => x.HeroId));
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error extracting startup heroes from data collected on {collectedAt}",
                        valueTuple.CollectedAt);
                }
            }

            logger.LogInformation("Starting player heroes service update");
            await ExecuteSafeAsync(() => playerHeroesService.RunAsync(allUnits, allHeroes, gameWorld.Id),
                $"Error while processing player heroes for game world {gameWorld.Id}.");
            logger.LogInformation("Completed player heroes service update");

            logger.LogInformation("Completed processing game world {gameWorldId}", gameWorld.Id);
        }

        return HasMoreData;
    }

    private static void Merge(Dictionary<int, HashSet<string>> target, int key, IEnumerable<string> values)
    {
        if (!target.TryGetValue(key, out var existing))
        {
            target[key] = new HashSet<string>(values);
        }
        else
        {
            existing.UnionWith(values);
        }
    }
}
