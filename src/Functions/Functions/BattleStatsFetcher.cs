using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Application.Server.Interfaces.Hoh;
using Ingweland.Fog.Application.Server.Providers;
using Ingweland.Fog.Application.Server.Services.Hoh.Abstractions;
using Ingweland.Fog.Functions.Services;
using Ingweland.Fog.InnSdk.Hoh.Abstractions;
using Ingweland.Fog.InnSdk.Hoh.Authentication.Models;
using Ingweland.Fog.InnSdk.Hoh.Providers;
using Ingweland.Fog.Models.Hoh.Entities.Battle;
using Ingweland.Fog.Shared.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Functions.Functions;

public class BattleStatsFetcher(
    IGameWorldsProvider gameWorldsProvider,
    IInGameRawDataTableRepository inGameRawDataTableRepository,
    IInGameDataParsingService inGameDataParsingService,
    InGameRawDataTablePartitionKeyProvider inGameRawDataTablePartitionKeyProvider,
    IInnSdkClient innSdkClient,
    IBattleStatsService battleStatsService,
    IFogDbContext context,
    ILogger<BattleStatsFetcher> logger) : FunctionBase(gameWorldsProvider, inGameRawDataTableRepository,
    inGameDataParsingService, inGameRawDataTablePartitionKeyProvider, logger)
{
    [Function("BattleStatsFetcher")]
    public async Task Run([TimerTrigger("0 50 7-23/1 * * *")] TimerInfo myTimer)
    {
        logger.LogInformation("BattleStatsFetcher started");

        var initDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);

        foreach (var gameWorld in GameWorldsProvider.GetGameWorlds())
        {
            var savedBattleStatsIds = await context.Battles.AsNoTracking()
                .Where(t => t.WorldId == gameWorld.Id && t.PerformedAt >= initDate)
                .Select(x => x.InGameBattleId)
                .ToHashSetAsync(StructuralByteArrayComparer.Instance);
            var existingBattleIds = await context.BattleStats.AsNoTracking()
                .Where(x => savedBattleStatsIds.Contains(x.InGameBattleId))
                .Select(x => x.InGameBattleId)
                .ToListAsync();
            var battlesWithoutStats = savedBattleStatsIds
                .Except(existingBattleIds, StructuralByteArrayComparer.Instance)
                .ToList();
            logger.LogInformation(
                "Game world {gameWorldId}. All battles: {count}, battles without stats: {statsCount}.",
                gameWorld.Id, savedBattleStatsIds.Count, battlesWithoutStats.Count);
            await FetchBattleStats(gameWorld, battlesWithoutStats);
        }

        logger.LogInformation("Completed battles stats fetch.");
    }

    private async Task FetchBattleStats(GameWorldConfig gameWorld, IEnumerable<byte[]> battleIds)
    {
        foreach (var battleId in battleIds)
        {
            var delayTask = Task.Delay(300);
            byte[] data;
            var battleIdString = Convert.ToBase64String(battleId);
            try
            {
                data = await innSdkClient.BattleService.GetBattleStatsRawDataAsync(gameWorld, battleId);
                logger.LogInformation("Fetch battle stats for world: {WorldId}, id: {BattleId}", gameWorld.Id,
                    battleIdString);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Could not fetch battle stats for world {WorldId}, id {BattleId}", gameWorld.Id,
                    battleIdString);
                continue;
            }

            BattleStats battleStats;
            try
            {
                battleStats = InGameDataParsingService.ParseBattleStats(Convert.ToBase64String(data));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error parsing battle stats raw data: world {WorldId}, id {BattleId}", gameWorld.Id,
                    battleIdString);
                continue;
            }

            await ExecuteSafeAsync(() => battleStatsService.AddAsync(battleStats),
                $"Error saving battle stats: world {gameWorld.Id}, id {battleIdString}");

            await delayTask;
        }
    }
}
