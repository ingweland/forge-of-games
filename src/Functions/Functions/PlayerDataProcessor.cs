using AutoMapper;
using Ingweland.Fog.Application.Server.Interfaces.Hoh;
using Ingweland.Fog.Application.Server.Providers;
using Ingweland.Fog.Application.Server.Services.Hoh.Abstractions;
using Ingweland.Fog.Functions.Services;
using Ingweland.Fog.InnSdk.Hoh.Providers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Functions.Functions;

public class PlayerDataProcessor(
    IGameWorldsProvider gameWorldsProvider,
    IPlayerRankingService playerRankingService,
    IInGameRawDataTableRepository inGameRawDataTableRepository,
    IInGameDataParsingService inGameDataParsingService,
    IPlayerService playerService,
    IPlayerAgeHistoryService playerAgeHistoryService,
    IPlayerNameHistoryService playerNameHistoryService,
    InGameRawDataTablePartitionKeyProvider inGameRawDataTablePartitionKeyProvider,
    IMapper mapper,
    ILogger<PlayerDataProcessor> logger,
    DatabaseWarmUpService databaseWarmUpService) : AutoDataProcessorBase(gameWorldsProvider,
    inGameRawDataTableRepository, inGameDataParsingService, inGameRawDataTablePartitionKeyProvider, mapper, logger,
    databaseWarmUpService)
{
    [Function(nameof(PlayerDataProcessor))]
    public async Task<bool> Run([ActivityTrigger] int input)
    {
        var data = await PrepareData(input);

        logger.LogInformation("Starting player service update");
        await ExecuteSafeAsync(() => playerService.AddAsync(data.PlayerAggregates), "");
        logger.LogInformation("Completed player service update");

        logger.LogInformation("Starting player ranking service update");
        await ExecuteSafeAsync(() => playerRankingService.AddOrUpdateRankingsAsync(data.PlayerAggregates), "");
        logger.LogInformation("Completed player ranking service update");

        logger.LogInformation("Starting player name history service update");
        await ExecuteSafeAsync(() => playerNameHistoryService.UpdateAsync(data.PlayerAggregates), "");
        logger.LogInformation("Completed player name history service update");

        logger.LogInformation("Starting player age history service update");
        await ExecuteSafeAsync(() => playerAgeHistoryService.UpdateAsync(data.PlayerAggregates), "");
        logger.LogInformation("Completed player age history service update");

        return HasMoreData;
    }
}
