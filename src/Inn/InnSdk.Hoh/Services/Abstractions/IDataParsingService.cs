using FluentResults;
using Ingweland.Fog.Inn.Models.Hoh;
using Ingweland.Fog.Models.Hoh.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Alliance;
using Ingweland.Fog.Models.Hoh.Entities.Battle;
using Ingweland.Fog.Models.Hoh.Entities.City;
using Ingweland.Fog.Models.Hoh.Entities.Ranking;
using Ingweland.Fog.Models.Hoh.Entities.Woa;

namespace Ingweland.Fog.InnSdk.Hoh.Services.Abstractions;

public interface IDataParsingService
{
    AllianceRanks ParseAllianceRankings(byte[] data);
    BattleStats ParseBattleStats(byte[] data);
    BattleSummary ParseBattleStart(byte[] data);
    BattleSummary ParseBattleWaveResult(byte[] data);
    IReadOnlyCollection<PvpBattle> ParsePvpBattles(byte[] data);
    IReadOnlyCollection<PvpRank> ParsePvpRankings(byte[] data);
    OtherCity ParseOtherCity(byte[] data);
    Result<PlayerRanks> ParsePlayerRankings(byte[] data);
    Result<IReadOnlyCollection<WoaPlayerStats>> ParseWoaPlayerStats(byte[] data);
    Result<AllianceWithMembers> ParseAllianceMembersResponse(byte[] data);
    Result<IReadOnlyCollection<AllianceWithLeader>> ParseSearchAllianceResponse(byte[] data);
    Result<AllianceWithLeader> ParseAllianceWithLeader(byte[] data);
    Result<BatchResponse> ParseBatchResponse(byte[] data);
    HeroFinishWaveRequestDto ParseBattleCompleteWaveRequest(byte[] data);

    Result<PlayerProfile> ParsePlayerProfile(byte[] data);
    Result<CommunicationDto> ParseCommunicationDto(byte[] data);
    Result<SoftErrorType?> GetSoftError(byte[] data);
    Wakeup ParseWakeup(byte[] data);
}
