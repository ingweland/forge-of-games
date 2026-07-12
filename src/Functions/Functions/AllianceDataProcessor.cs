using AutoMapper;
using Ingweland.Fog.Application.Server.Interfaces.Hoh;
using Ingweland.Fog.Application.Server.Providers;
using Ingweland.Fog.Application.Server.Services.Hoh.Abstractions;
using Ingweland.Fog.Functions.Services;
using Ingweland.Fog.InnSdk.Hoh.Providers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Functions.Functions;

public class AllianceDataProcessor(
    IGameWorldsProvider gameWorldsProvider,
    IInGameRawDataTableRepository inGameRawDataTableRepository,
    IInGameDataParsingService inGameDataParsingService,
    IAllianceService allianceService,
    IAllianceRankingService allianceRankingService,
    IAllianceNameHistoryService allianceNameHistoryService,
    IAllianceMembersService allianceMembersService,
    InGameRawDataTablePartitionKeyProvider inGameRawDataTablePartitionKeyProvider,
    IMapper mapper,
    ILogger<AllianceDataProcessor> logger,
    DatabaseWarmUpService databaseWarmUpService) : AutoDataProcessorBase(gameWorldsProvider,
    inGameRawDataTableRepository, inGameDataParsingService, inGameRawDataTablePartitionKeyProvider, mapper, logger,
    databaseWarmUpService)
{
    [Function(nameof(AllianceDataProcessor))]
    public async Task<bool> Run([ActivityTrigger] int input)
    {
        var data = await PrepareData(input);

        logger.LogInformation("Starting alliance service update");
        await ExecuteSafeAsync(() => allianceService.AddAsync(data.AllianceAggregates), "");
        logger.LogInformation("Completed alliance service update");

        logger.LogInformation("Starting alliance ranking service update");
        await ExecuteSafeAsync(() => allianceRankingService.AddOrUpdateRankingsAsync(data.AllianceAggregates), "");
        logger.LogInformation("Completed alliance ranking service update");

        logger.LogInformation("Starting alliance name history service update");
        await ExecuteSafeAsync(() => allianceNameHistoryService.UpdateAsync(data.AllianceAggregates), "");
        logger.LogInformation("Completed alliance name history service update");

        logger.LogInformation("Starting alliance members service update");
        await ExecuteSafeAsync(() => allianceMembersService.UpdateAsync(data.ConfirmedAllianceMembers), "");
        logger.LogInformation("Completed alliance members service update");

        return HasMoreData;
    }
}
