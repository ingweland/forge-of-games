using FluentResults;
using Ingweland.Fog.Inn.Models.Hoh;
using Ingweland.Fog.Models.Hoh.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Battle;
using Ingweland.Fog.Models.Hoh.Entities.City;
using Ingweland.Fog.Models.Hoh.Entities.Ranking;
using Ingweland.Fog.Models.Hoh.Entities.Woa;

namespace Ingweland.Fog.Application.Server.Services.Hoh.Abstractions;

public interface IInGameDataParsingService
{
    AllianceRanks ParseAllianceRankings(string inputData);
    BattleSummary ParseBattleWaveResult(string inputData);
    BattleStats ParseBattleStats(string inputData);
    byte[] Decode(string inputData);
    IReadOnlyCollection<PvpBattle> ParsePvpBattles(string inputData);
    IReadOnlyCollection<PvpRank> ParsePvpRanking(string inputData);
    HeroFinishWaveRequestDto ParseBattleCompleteWaveRequest(string inputData);
    Result<PlayerRanks> ParsePlayerRanking(string inputData);
    Result<IReadOnlyCollection<WoaPlayerStats>> ParseWoaPlayerStats(string inputData);
    Wakeup ParseWakeup(string inputData);
    OtherCity ParseOtherCity(string inputData);
    Result<SoftErrorType?> GetSoftError(string inputData);
}
