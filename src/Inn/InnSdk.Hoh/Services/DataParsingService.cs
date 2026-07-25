using AutoMapper;
using FluentResults;
using Google.Protobuf;
using Ingweland.Fog.Inn.Models.Hoh;
using Ingweland.Fog.Inn.Models.Hoh.Errors;
using Ingweland.Fog.Inn.Models.Hoh.Extensions;
using Ingweland.Fog.InnSdk.Hoh.Errors;
using Ingweland.Fog.InnSdk.Hoh.Services.Abstractions;
using Ingweland.Fog.Models.Hoh.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Alliance;
using Ingweland.Fog.Models.Hoh.Entities.Battle;
using Ingweland.Fog.Models.Hoh.Entities.City;
using Ingweland.Fog.Models.Hoh.Entities.Ranking;
using Ingweland.Fog.Models.Hoh.Entities.Woa;

namespace Ingweland.Fog.InnSdk.Hoh.Services;

public class DataParsingService(IMapper mapper) : IDataParsingService
{
    public Result<PlayerRanks> ParsePlayerRankings(byte[] data)
    {
        return ParseCommunicationDto<PlayerRanksDTO>(data)
            .Bind(dto =>
            {
                return Result.Try(() => mapper.Map<PlayerRanks>(dto),
                    e => new HohMappingError($"Failed to map {nameof(PlayerRanksDTO)} to {nameof(PlayerRanks)}", e));
            });
    }

    public Result<IReadOnlyCollection<WoaPlayerStats>> ParseWoaPlayerStats(byte[] data)
    {
        return ParseCommunicationDto<WoAGetPlayerStatsResponse>(data)
            .Bind(dto =>
            {
                return Result.Try(() => mapper.Map<IReadOnlyCollection<WoaPlayerStats>>(dto.Entries),
                    e => new HohMappingError(
                        $"Failed to map {nameof(WoAPlayerStatsDTO)} to {nameof(WoaPlayerStats)}", e));
            });
    }

    public Result<IReadOnlyCollection<AllianceWithLeader>> ParseSearchAllianceResponse(byte[] data)
    {
        return ParseCommunicationDto<SearchAllianceResponse>(data)
            .Bind(dto =>
            {
                return Result.Try(() => mapper.Map<IReadOnlyCollection<AllianceWithLeader>>(dto.Alliances),
                    e => new HohMappingError(
                        $"Failed to map {nameof(SearchAllianceResponse.Alliances)} to {
                            nameof(IReadOnlyCollection<AllianceWithLeader>)}", e));
            });
    }

    public Result<AllianceWithLeader> ParseAllianceWithLeader(byte[] data)
    {
        return ParseCommunicationDto<AllianceWithLeaderDTO>(data)
            .Bind(dto =>
            {
                return Result.Try(() => mapper.Map<AllianceWithLeader>(dto),
                    e => new HohMappingError(
                        $"Failed to map {nameof(AllianceWithLeaderDTO)} to {nameof(AllianceWithLeader)}", e));
            });
    }

    public Result<BatchResponse> ParseBatchResponse(byte[] data)
    {
        return ParseCommunicationDto<BatchResponse>(data);
    }

    public HeroFinishWaveRequestDto ParseBattleCompleteWaveRequest(byte[] data)
    {
        try
        {
            return HeroFinishWaveRequestDto.Parser.ParseFrom(data);
        }
        catch (Exception ex)
        {
            const string msg = "Failed to parse hero finish wave request data";
            throw new InvalidOperationException(msg, ex);
        }
    }

    public Result<PlayerProfile> ParsePlayerProfile(byte[] data)
    {
        return ParseCommunicationDto<PlayerProfileResponse>(data)
            .Bind(dto =>
            {
                return Result.Try(() => mapper.Map<PlayerProfile>(dto),
                    e => new HohMappingError(
                        $"Failed to map {nameof(PlayerProfileResponse)} to {nameof(PlayerProfile)}", e));
            });
    }

    public AllianceRanks ParseAllianceRankings(byte[] data)
    {
        AllianceRanksDTO ranksDto;
        try
        {
            var container = CommunicationDto.Parser.ParseFrom(data);
            ranksDto = container.AllianceRanks;
        }
        catch (Exception ex)
        {
            const string msg = "Failed to parse alliance ranking data";
            throw new InvalidOperationException(msg, ex);
        }

        return mapper.Map<AllianceRanks>(ranksDto);
    }

    public Result<CommunicationDto> ParseCommunicationDto(byte[] data)
    {
        return Result.Try(() => CommunicationDto.Parser.ParseFrom(data),
            e => new HohProtobufParsingError(ProtobufParsingStage.BinaryDeserialization, nameof(CommunicationDto), e));
    }

    public Result<SoftErrorType?> GetSoftError(byte[] data)
    {
        var communicationDto = Result.Try(() => CommunicationDto.Parser.ParseFrom(data),
            e => new HohProtobufParsingError(ProtobufParsingStage.BinaryDeserialization, nameof(CommunicationDto), e));
        if (communicationDto.IsFailed)
        {
            return communicationDto.ToResult();
        }

        return communicationDto.Value.HasError
            ? Result.Ok<SoftErrorType?>(communicationDto.Value.Error)
            : Result.Ok<SoftErrorType?>(null);
    }

    public BattleSummary ParseBattleStart(byte[] data)
    {
        HeroStartBattleResponse dto;
        try
        {
            var container = CommunicationDto.Parser.ParseFrom(data);
            dto = container.HeroStartBattleResponse;
        }
        catch (Exception ex)
        {
            const string msg = "Failed to parse hero start battle response data";
            throw new InvalidOperationException(msg, ex);
        }

        return mapper.Map<BattleSummary>(dto.Result);
    }

    public IReadOnlyCollection<PvpRank> ParsePvpRankings(byte[] data)
    {
        PvpGetRankingResponse ranksDto;
        try
        {
            var container = CommunicationDto.Parser.ParseFrom(data);
            ranksDto = container.PvpGetRankingResponse;
        }
        catch (Exception ex)
        {
            const string msg = "Failed to parse pvp ranking data";
            throw new InvalidOperationException(msg, ex);
        }

        return mapper.Map<IReadOnlyCollection<PvpRank>>(ranksDto.Rankings);
    }

    public BattleSummary ParseBattleWaveResult(byte[] data)
    {
        HeroFinishWaveResponse dto;
        try
        {
            var container = CommunicationDto.Parser.ParseFrom(data);
            dto = container.HeroFinishWaveResponse;
        }
        catch (Exception ex)
        {
            const string msg = "Failed to parse hero finish wave response data";
            throw new InvalidOperationException(msg, ex);
        }

        return mapper.Map<BattleSummary>(dto.Result);
    }

    public IReadOnlyCollection<PvpBattle> ParsePvpBattles(byte[] data)
    {
        PvpBattleHistoryResponse battlesDto;
        try
        {
            var container = CommunicationDto.Parser.ParseFrom(data);
            battlesDto = container.PvpBattleHistoryResponse!;
        }
        catch (Exception ex)
        {
            const string msg = "Failed to parse pvp battles data";
            throw new InvalidOperationException(msg, ex);
        }

        return (from battleDto in battlesDto.History
            let firstResult = battleDto.StageResults.First()
            let winnerDto = firstResult.Won ? battleDto.Attacker : battleDto.Target
            let winnerUnitsDto = firstResult.Won ? firstResult.PlayerUnits : firstResult.EnemyUnits
            let loserDto = firstResult.Won ? battleDto.Target : battleDto.Attacker
            let loserUnitsDto = firstResult.Won ? firstResult.EnemyUnits : firstResult.PlayerUnits
            let winner = mapper.Map<HohPlayer>(winnerDto)
            let winnerUnits = mapper.Map<IReadOnlyCollection<BattleSquad>>(winnerUnitsDto)
            let loser = mapper.Map<HohPlayer>(loserDto)
            let loserUnits = mapper.Map<IReadOnlyCollection<BattleSquad>>(loserUnitsDto)
            select new PvpBattle
            {
                Id = firstResult.BattleId.ToByteArray(),
                Winner = winner,
                Loser = loser,
                WinnerUnits = winnerUnits,
                LoserUnits = loserUnits,
                PerformedAt = battleDto.BattleEnded.ToDateTime(),
            }).ToList();
    }

    public Wakeup ParseWakeup(byte[] data)
    {
        CommunicationDto dto;
        try
        {
            dto = CommunicationDto.Parser.ParseFrom(data);
        }
        catch (Exception ex)
        {
            const string msg = "Failed to parse alliance member data";
            throw new InvalidOperationException(msg, ex);
        }

        return mapper.Map<Wakeup>(dto);
    }

    public BattleStats ParseBattleStats(byte[] data)
    {
        HeroBattleStatsResponse dto;
        try
        {
            var container = CommunicationDto.Parser.ParseFrom(data);
            dto = container.HeroBattleStatsResponse;
        }
        catch (Exception ex)
        {
            const string msg = "Failed to parse hero battle stats response data";
            throw new InvalidOperationException(msg, ex);
        }

        return mapper.Map<BattleStats>(dto);
    }

    public OtherCity ParseOtherCity(byte[] data)
    {
        OtherCityDTO dto;
        try
        {
            var container = CommunicationDto.Parser.ParseFrom(data);
            dto = container.OtherCity;
        }
        catch (Exception ex)
        {
            const string msg = "Failed to parse other city data";
            throw new InvalidOperationException(msg, ex);
        }

        return mapper.Map<OtherCity>(dto);
    }

    public Result<AllianceWithMembers> ParseAllianceMembersResponse(byte[] data)
    {
        return ParseCommunicationDto<AllianceMembersResponse>(data)
            .Bind(dto =>
            {
                return Result.Try(() => mapper.Map<AllianceWithMembers>(dto),
                    e => new HohMappingError(
                        $"Failed to map {nameof(AllianceMembersResponse)} members to {nameof(AllianceWithMembers)}",
                        e));
            });
    }

    private static Result<T> ParseCommunicationDto<T>(byte[] data) where T : IMessage<T>, new()
    {
        var communicationDto = Result.Try(() => CommunicationDto.Parser.ParseFrom(data),
            e => new HohProtobufParsingError(ProtobufParsingStage.BinaryDeserialization, nameof(CommunicationDto), e));
        if (communicationDto.IsFailed)
        {
            return communicationDto.ToResult<T>();
        }

        return communicationDto.Value.HasError
            ? Result.Fail<T>(new HohSoftError(communicationDto.Value.Error))
            : communicationDto.Value.Response.FindAndUnpackToResult<T>();
    }
}
