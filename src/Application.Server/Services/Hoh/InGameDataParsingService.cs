using FluentResults;
using Ingweland.Fog.Application.Server.Errors;
using Ingweland.Fog.Application.Server.Services.Hoh.Abstractions;
using Ingweland.Fog.Inn.Models.Hoh;
using Ingweland.Fog.InnSdk.Hoh.Services.Abstractions;
using Ingweland.Fog.Models.Hoh.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Battle;
using Ingweland.Fog.Models.Hoh.Entities.City;
using Ingweland.Fog.Models.Hoh.Entities.Ranking;
using Ingweland.Fog.Models.Hoh.Entities.Woa;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Application.Server.Services.Hoh;

public class InGameDataParsingService(
    IDataParsingService dataParsingService,
    ILogger<InGameDataProcessingServiceBase> logger)
    : InGameDataProcessingServiceBase(logger), IInGameDataParsingService
{
    public HeroFinishWaveRequestDto ParseBattleCompleteWaveRequest(string inputData)
    {
        var data = DecodeInternal(inputData);

        return dataParsingService.ParseBattleCompleteWaveRequest(data);
    }

    public Result<PlayerRanks> ParsePlayerRanking(string inputData)
    {
        return TryDecodeAndBind(inputData, dataParsingService.ParsePlayerRankings);
    }

    public Result<CommunicationDto> ParseCommunicationDto(string inputData)
    {
        return TryDecodeAndBind(inputData, dataParsingService.ParseCommunicationDto);
    }

    public Result<IReadOnlyCollection<WoaPlayerStats>> ParseWoaPlayerStats(string inputData)
    {
        return TryDecodeAndBind(inputData, dataParsingService.ParseWoaPlayerStats);
    }

    public IReadOnlyCollection<PvpRank> ParsePvpRanking(string inputData)
    {
        var data = DecodeInternal(inputData);

        return dataParsingService.ParsePvpRankings(data);
    }

    public IReadOnlyCollection<PvpBattle> ParsePvpBattles(string inputData)
    {
        var data = DecodeInternal(inputData);

        return dataParsingService.ParsePvpBattles(data);
    }

    public BattleSummary ParseBattleWaveResult(string inputData)
    {
        var data = DecodeInternal(inputData);

        return dataParsingService.ParseBattleWaveResult(data);
    }

    public BattleStats ParseBattleStats(string inputData)
    {
        var data = DecodeInternal(inputData);

        return dataParsingService.ParseBattleStats(data);
    }

    public AllianceRanks ParseAllianceRankings(string inputData)
    {
        var data = DecodeInternal(inputData);

        return dataParsingService.ParseAllianceRankings(data);
    }

    public Wakeup ParseWakeup(string inputData)
    {
        var data = DecodeInternal(inputData);

        return dataParsingService.ParseWakeup(data);
    }

    public OtherCity ParseOtherCity(string inputData)
    {
        var data = DecodeInternal(inputData);

        return dataParsingService.ParseOtherCity(data);
    }

    public byte[] Decode(string inputData)
    {
        return DecodeInternal(inputData);
    }

    public Result<SoftErrorType?> GetSoftError(string inputData)
    {
        return TryDecodeAndBind(inputData, dataParsingService.GetSoftError);
    }

    private Result<T> TryDecodeAndBind<T>(string inputData, Func<byte[], Result<T>> bindFunc)
    {
        return Result
            .Try(() => DecodeInternal(inputData), e => new InGameDataDecodingError(e))
            .Bind(bindFunc);
    }
}
