using Ingweland.Fog.Application.Server.Interfaces.Hoh;
using Ingweland.Fog.Application.Server.Providers;
using Ingweland.Fog.Application.Server.Services.Hoh.Abstractions;
using Ingweland.Fog.Functions.Services;
using Ingweland.Fog.InnSdk.Hoh.Providers;
using Ingweland.Fog.Models.Hoh.Entities.Woa;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Functions.Functions;

public class WoaPlayerStatsProcessor(
    IGameWorldsProvider gameWorldsProvider,
    IInGameRawDataTableRepository inGameRawDataTableRepository,
    IInGameDataParsingService inGameDataParsingService,
    InGameRawDataTablePartitionKeyProvider inGameRawDataTablePartitionKeyProvider,
    IWoaPlayerStatsService woaPlayerStatsService,
    ILogger<WoaPlayerStatsProcessor> logger,
    DatabaseWarmUpService databaseWarmUpService) : FunctionBase(gameWorldsProvider, inGameRawDataTableRepository,
    inGameDataParsingService, inGameRawDataTablePartitionKeyProvider, logger)
{
    private const int BATCH_SIZE = 3000;
    private bool _hasMoreData;

    [Function(nameof(WoaPlayerStatsProcessor))]
    public async Task<bool> Run([ActivityTrigger] int dataPage)
    {
        logger.LogInformation("{activity} started.", nameof(WoaPlayerStatsProcessor));
        await databaseWarmUpService.WarmUpDatabaseIfRequiredAsync();

        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        logger.LogInformation("BattlesProcessor started for date {date}", date);
        foreach (var gameWorld in GameWorldsProvider.GetGameWorlds())
        {
            logger.LogInformation("Processing game world {gameWorldId}", gameWorld.Id);

            var stats = await GetWoaPlayerStats(gameWorld.Id, date, dataPage);

            logger.LogInformation("Completed processing game world {gameWorldId}", gameWorld.Id);

            logger.LogInformation("Starting woa player stats service update");
            await ExecuteSafeAsync(() => woaPlayerStatsService.RunAsync(stats, gameWorld.Id),
                $"Error while processing woa player stats for game world {gameWorld.Id}.");
            logger.LogInformation("Completed woa player stats service update");
        }

        return _hasMoreData;
    }

    private async Task<List<(DateTime CollectedAt, WoaPlayerStats Stats)>> GetWoaPlayerStats(string worldId,
        DateOnly date, int dataPage = -1)
    {
        var statsRawData = await ExecuteSafeAsync(async () =>
        {
            var partitionKey = InGameRawDataTablePartitionKeyProvider.WoaPlayerStats(worldId, date);
            if (dataPage < 0)
            {
                return await InGameRawDataTableRepository.GetAllAsync(partitionKey);
            }

            var result = await InGameRawDataTableRepository.GetAsync(partitionKey, BATCH_SIZE * dataPage,
                BATCH_SIZE);
            if (result.Count >= BATCH_SIZE)
            {
                _hasMoreData = true;
            }

            return result;
        }, "", []);

        var allStatItems = new List<(DateTime CollectedAt, WoaPlayerStats Stats)>();
        foreach (var rawData in statsRawData)
        {
            var stats = InGameDataParsingService.ParseWoaPlayerStats(rawData.Base64Data);
            stats.LogIfFailed<WoaPlayerStatsProcessor>();
            if (stats.IsSuccess)
            {
                allStatItems.AddRange(stats.Value.Select(x => (rawData.CollectedAt, x)));
            }
        }

        return allStatItems;
    }
}
