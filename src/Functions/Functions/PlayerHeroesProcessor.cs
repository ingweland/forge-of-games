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

            var data = await GetDataAsync(InGameRawDataTablePartitionKeyProvider.HeroesWakeup(gameWorld.Id, date),
                dataPage);
            var allUnits = new Dictionary<int, HashSet<string>>();
            var allHeroes = new Dictionary<int, HashSet<string>>();
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
                            if (!allUnits.TryGetValue(kvp.Key, out var existing))
                            {
                                allUnits[kvp.Key] = new HashSet<string>(kvp.Value);
                            }
                            else
                            {
                                existing.UnionWith(kvp.Value);
                            }
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
                    if (!allHeroes.TryGetValue(kvp.Key, out var existing))
                    {
                        allHeroes[kvp.Key] = new HashSet<string>(kvp.Value);
                    }
                    else
                    {
                        existing.UnionWith(kvp.Value);
                    }
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
}
