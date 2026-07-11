using System.Text.Json;
using Azure.Storage.Queues.Models;
using Ingweland.Fog.Application.Server.Interfaces.Hoh;
using Ingweland.Fog.Application.Server.Services.Hoh.Abstractions;
using Ingweland.Fog.Functions.Services;
using Ingweland.Fog.Infrastructure.Entities;
using Ingweland.Fog.Infrastructure.Enums;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Battle;
using Ingweland.Fog.Models.Hoh.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Functions.Functions;

public class InGameDataQueueProcessor(
    IInGameRawDataTableRepository inGameRawDataTableRepository,
    IInGameDataParsingService inGameDataParsingService,
    IBattleService battleService,
    IBattleTimelineService battleTimelineService,
    DatabaseWarmUpService databaseWarmUpService,
    IAllianceAthService allianceAthService,
    IPlayerAthService playerAthService,
    IAllianceWoaService allianceWoaService,
    ILogger<InGameDataQueueProcessor> logger)
{
    [Function(nameof(InGameDataQueueProcessor))]
    public async Task Run(
        [QueueTrigger("%StorageSettings:InGameRawDataProcessingQueue%",
            Connection = "StorageSettings:ConnectionString")]
        QueueMessage message)
    {
        var payload = JsonSerializer.Deserialize<InGameRawDataQueueMessage>(message.MessageText);
        if (payload == null)
        {
            throw new Exception("Could not deserialize payload");
        }

        logger.LogInformation("ST: {ServiceType}, PK: {MessageMessageText}, RK: {RowKey}",
            payload.ProcessingServiceType.ToString(), payload.PartitionKey, payload.RowKey);

        switch (payload.ProcessingServiceType)
        {
            case InGameDataProcessingServiceType.Battle:
            {
                await ProcessBattleAsync(payload.PartitionKey, payload.RowKey);
                break;
            }
            case InGameDataProcessingServiceType.WakeupLeaderboards:
            {
                await ProcessAllianceAthRankingsAsync(payload.PartitionKey, payload.RowKey);
                break;
            }
            case InGameDataProcessingServiceType.WakeupAlliance:
            {
                await ProcessPlayerAthRankingsAsync(payload.PartitionKey, payload.RowKey);
                break;
            }
            case InGameDataProcessingServiceType.WakeupAllianceWoa:
            {
                await ProcessAllianceWoaRankingsAsync(payload.PartitionKey, payload.RowKey);
                break;
            }
            default:
            {
                throw new Exception($"Unknown processing service type {payload.ProcessingServiceType}");
            }
        }
    }

    private async Task ProcessBattleAsync(string partitionKey, string rowKey)
    {
        var rawData = await LoadRawDataAsync(partitionKey, rowKey);

        var parsedResult = inGameDataParsingService.ParseBattleWaveResult(rawData.Base64Data);

        if (parsedResult.Location is PvpRevengeBattleLocation)
        {
            return;
        }

        if (parsedResult.ResultStatus != BattleResultStatus.Undefined)
        {
            var parts = partitionKey.Split('_');
            await battleService.AddAsync(parts[1], parsedResult, DateOnly.ParseExact(parts[2], "yyyy-MM-dd"),
                rawData.SubmissionId);
        }

        if (rawData.RequestBase64Data != null)
        {
            var parsedRequest = inGameDataParsingService.ParseBattleCompleteWaveRequest(rawData.RequestBase64Data);
            await battleTimelineService.UpsertAsync(parsedRequest);
        }
    }

    private async Task ProcessAllianceAthRankingsAsync(string partitionKey, string rowKey)
    {
        var rawData = await LoadRawDataAsync(partitionKey, rowKey);
        var parts = partitionKey.Split('_');
        var wakeup = inGameDataParsingService.ParseWakeup(rawData.Base64Data);
        await allianceAthService.RunAsync(wakeup.AthAllianceRankings, parts[1], rawData.CollectedAt);
    }

    private async Task ProcessAllianceWoaRankingsAsync(string partitionKey, string rowKey)
    {
        var rawData = await LoadRawDataAsync(partitionKey, rowKey);
        var parts = partitionKey.Split('_');
        var wakeup = inGameDataParsingService.ParseWakeup(rawData.Base64Data);
        await allianceWoaService.RunAsync(wakeup.WoaDivision, wakeup.WoaAlliances, wakeup.Alliances, parts[1],
            rawData.CollectedAt);
    }

    private async Task ProcessPlayerAthRankingsAsync(string partitionKey, string rowKey)
    {
        var rawData = await LoadRawDataAsync(partitionKey, rowKey);
        var parts = partitionKey.Split('_');
        var wakeup = inGameDataParsingService.ParseWakeup(rawData.Base64Data);
        await playerAthService.RunAsync(wakeup.AthPlayerRankings, parts[1], rawData.CollectedAt);
    }

    private async Task<InGameRawData> LoadRawDataAsync(string partitionKey, string rowKey)
    {
        await databaseWarmUpService.WarmUpDatabaseIfRequiredAsync();

        var rawData = await inGameRawDataTableRepository.GetAsync(partitionKey, rowKey);
        if (rawData == null)
        {
            throw new Exception($"Could not find raw data for partition key {partitionKey} and row key {rowKey}");
        }

        return rawData;
    }
}
