using Ingweland.Fog.Application.Server.Interfaces.Hoh;
using Ingweland.Fog.Application.Server.Providers;
using Ingweland.Fog.Application.Server.Services.Hoh.Abstractions;
using Ingweland.Fog.Inn.Models.Hoh;
using Ingweland.Fog.InnSdk.Hoh.Providers;
using Ingweland.Fog.Models.Hoh.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Battle;
using Microsoft.Extensions.Logging;
using BattleResultStatus = Ingweland.Fog.Models.Hoh.Enums.BattleResultStatus;

namespace Ingweland.Fog.Functions.Functions;

public class FunctionBase(
    IGameWorldsProvider gameWorldsProvider,
    IInGameRawDataTableRepository inGameRawDataTableRepository,
    IInGameDataParsingService inGameDataParsingService,
    InGameRawDataTablePartitionKeyProvider inGameRawDataTablePartitionKeyProvider,
    ILogger logger)
{
    private const int BATCH_SIZE = 100;
    protected IGameWorldsProvider GameWorldsProvider { get; } = gameWorldsProvider;
    protected bool HasMoreData { get; private set; }

    protected IInGameDataParsingService InGameDataParsingService { get; } = inGameDataParsingService;

    protected InGameRawDataTablePartitionKeyProvider InGameRawDataTablePartitionKeyProvider { get; } =
        inGameRawDataTablePartitionKeyProvider;

    protected IInGameRawDataTableRepository InGameRawDataTableRepository { get; } = inGameRawDataTableRepository;

    protected async Task<List<(string WorldId, BattleSummary BattleSummary, DateOnly PerformedAt, Guid? SubmissionId)>>
        GetBattleResults(string worldId,
            DateOnly date)
    {
        var rawDataItems = await ExecuteSafeAsync(
            () => InGameRawDataTableRepository.GetAllAsync(
                InGameRawDataTablePartitionKeyProvider.BattleCompleteWave(worldId, date)),
            $"Error getting battle complete wave raw data for world {worldId} on {date}", []);
        var result =
            new List<(string WorldId, BattleSummary BattleSummary, DateOnly PerformedAt, Guid? SubmissionId)>();
        foreach (var rawData in rawDataItems)
        {
            try
            {
                var parsed = InGameDataParsingService.ParseBattleWaveResult(rawData.Base64Data);

                if (parsed.Location is PvpRevengeBattleLocation)
                {
                    continue;
                }

                if (parsed.ResultStatus == BattleResultStatus.Undefined)
                {
                    continue;
                }

                result.Add((worldId, parsed, date, rawData.SubmissionId));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error parsing battle complete wave raw data collected on {date}",
                    rawData.CollectedAt);
            }
        }

        return result;
    }

    protected async IAsyncEnumerable<HeroFinishWaveRequestDto> GetBattleRequests(string worldId, DateOnly date)
    {
        var rawDataItems = await ExecuteSafeAsync(
            () => InGameRawDataTableRepository.GetAllAsync(
                InGameRawDataTablePartitionKeyProvider.BattleCompleteWave(worldId, date)),
            $"Error getting battle complete wave raw data for world {worldId} on {date}", []);
        foreach (var rawData in rawDataItems)
        {
            if (rawData.RequestBase64Data != null)
            {
                HeroFinishWaveRequestDto? parsedRequest = null;
                try
                {
                    parsedRequest = InGameDataParsingService.ParseBattleCompleteWaveRequest(rawData.RequestBase64Data);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error parsing battle complete wave request raw data collected on {date}",
                        rawData.CollectedAt);
                }

                if (parsedRequest != null)
                {
                    yield return parsedRequest;
                }
            }
        }
    }

    protected async Task<List<(string worldId, BattleStats battleStats)>> GetBattleStats(string worldId,
        DateOnly date)
    {
        var rawDataItems = await ExecuteSafeAsync(
            () => InGameRawDataTableRepository.GetAllAsync(
                InGameRawDataTablePartitionKeyProvider.BattleStats(worldId, date)),
            $"Error getting battle stats raw data for world {worldId} on {date}", []);
        var result = new List<(string worldId, BattleStats battleStats)>();
        foreach (var rawData in rawDataItems)
        {
            try
            {
                result.Add((worldId, InGameDataParsingService.ParseBattleStats(rawData.Base64Data)));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error parsing battle stats raw data collected on {date}",
                    rawData.CollectedAt);
            }
        }

        return result;
    }

    protected async Task<List<(string WorldId, PvpBattle PvpBattle)>> GetPvpBattles(string worldId, DateOnly date)
    {
        var pvpBattlesRawData = await ExecuteSafeAsync(
            () => InGameRawDataTableRepository.GetAllAsync(
                InGameRawDataTablePartitionKeyProvider.PvpBattles(worldId, date)),
            $"Error getting pvp battles raw data for world {worldId} on {date}", []);
        var pvpBattles = new List<(string WorldId, PvpBattle Battle)>();
        foreach (var rawData in pvpBattlesRawData)
        {
            try
            {
                pvpBattles.AddRange(InGameDataParsingService.ParsePvpBattles(rawData.Base64Data)
                    .Select(src => (worldId, src)));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error parsing pvp battles raw data collected on {date}", rawData.CollectedAt);
            }
        }

        return pvpBattles;
    }

    protected async Task ExecuteSafeAsync(Func<Task> func, string errorMessage)
    {
        try
        {
            await func();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while executing safe operation: {ErrorMessage}", errorMessage);
        }
    }

    protected async Task<T> ExecuteSafeAsync<T>(Func<Task<T>> func, string errorMessage, T fallback)
    {
        try
        {
            return await func();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while executing safe operation: {ErrorMessage}", errorMessage);
            return fallback;
        }
    }

    protected async Task<List<(DateTime CollectedAt, Wakeup Wakeup)>> GetWakeupsAsync(string partitionKey,
        int wakeupPage = -1)
    {
        var rawData = await ExecuteSafeAsync(async () =>
        {
            if (wakeupPage >= 0)
            {
                var result = await InGameRawDataTableRepository.GetAsync(partitionKey, BATCH_SIZE * wakeupPage,
                    BATCH_SIZE);
                if (result.Count >= BATCH_SIZE)
                {
                    HasMoreData = true;
                }

                return result;
            }

            return await InGameRawDataTableRepository.GetAllAsync(partitionKey);
        }, "", []);
        var alliances = new List<(DateTime CollectedAt, Wakeup Wakeup)>();
        foreach (var rd in rawData)
        {
            try
            {
                alliances.Add((rd.CollectedAt, InGameDataParsingService.ParseWakeup(rd.Base64Data)));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error parsing wakeup raw data collected on {CollectedAt}", rd.CollectedAt);
            }
        }

        return alliances;
    }

    protected async Task<List<(DateTime CollectedAt, CommunicationDto CommunicationDto)>> GetDataAsync(
        string partitionKey,
        int page = -1)
    {
        var rawData = await ExecuteSafeAsync(async () =>
        {
            if (page < 0)
            {
                return await InGameRawDataTableRepository.GetAllAsync(partitionKey);
            }

            var result = await InGameRawDataTableRepository.GetAsync(partitionKey, BATCH_SIZE * page,
                BATCH_SIZE);
            if (result.Count >= BATCH_SIZE)
            {
                HasMoreData = true;
            }

            return result;
        }, "", []);
        var data = new List<(DateTime CollectedAt, CommunicationDto CommunicationDto)>();
        foreach (var rd in rawData)
        {
            var result = InGameDataParsingService.ParseCommunicationDto(rd.Base64Data);
            result.LogIfFailed<FunctionBase>($"Error parsing communication dto raw data collected on {rd.CollectedAt}");
            if (result.IsSuccess)
            {
                data.Add((rd.CollectedAt, result.Value));
            }
        }

        return data;
    }
}
