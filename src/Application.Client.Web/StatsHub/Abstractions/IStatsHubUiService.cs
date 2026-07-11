using Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh.Battle;
using Ingweland.Fog.Dtos.Hoh.Battle;
using Ingweland.Fog.Dtos.Hoh.Stats;
using Ingweland.Fog.Models.Fog;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.StatsHub.Abstractions;

public interface IStatsHubUiService
{
    Task<PlayerProfileViewModel?> GetPlayerProfileAsync(int playerId);
    Task<PlayerViewModel?> GetPlayerAsync(int playerId, CancellationToken ct = default);

    Task<PaginatedList<PvpBattleViewModel>> GetPlayerBattlesAsync(PlayerViewModel player, int startIndex, int count,
        CancellationToken ct = default);

    Task<PaginatedList<PlayerViewModel>> GetPlayerStatsAsync(string worldId, int startIndex, int pageSize,
        string? playerName = null, CancellationToken ct = default);

    Task<AllianceProfileViewModel?> GetAllianceAsync(int allianceId);

    Task<PaginatedList<AllianceViewModel>> GetAllianceStatsAsync(string worldId, int startIndex, int pageSize,
        string? allianceName = null, CancellationToken ct = default);

    Task<IReadOnlyList<BattleSummaryViewModel>> SearchBattles(BattleSearchRequest request,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<AllianceAthRankingViewModel>> GetAllianceAthRankingsAsync(int allianceId);

    Task<IReadOnlyCollection<PlayerAthRankingViewModel>> GetPlayerAthRankingsAsync(int playerId,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<WonderRankingViewModel>> GetWonderRankingsAsync(int playerId);
    Task<IReadOnlyCollection<PvpRankingViewModel>> GetPlayerPvpRankingsAsync(int playerId);

    Task<PaginatedList<AllianceViewModel>> GetAlliancesAthRankingsAsync(string worldId, int startIndex, int pageSize,
        TreasureHuntLeague league, CancellationToken ct = default);

    Task<PaginatedList<PlayerViewModel>> GetEventCityRankingsAsync(string worldId, CancellationToken ct = default);
    Task<IReadOnlyCollection<PlayerViewModel>> GetTopPlayersAsync(string worldId, CancellationToken ct = default);
    Task<IReadOnlyCollection<PlayerViewModel>> GetTopEventCitiesAsync(string worldId, CancellationToken ct = default);
    Task<IReadOnlyCollection<AllianceViewModel>> GetTopAlliancesAsync(string worldId, CancellationToken ct = default);

    Task<IReadOnlyCollection<AllianceViewModel>> GetTopAlliancesAthRankingsAsync(string worldId,
        TreasureHuntLeague league, CancellationToken ct = default);

    Task<IReadOnlyCollection<StatsTimedIntValue>> GetAllianceRankingsAsync(int allianceId,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<StatsTimedIntValue>> GetPlayerRankingsAsync(int playerId,
        CancellationToken ct = default);

    Task<PlayerCityPropertiesViewModel?> GetPlayerCityPropertiesAsync(int playerId,
        CancellationToken ct = default);
    Task<IReadOnlyCollection<AllianceWoaRankingViewModel>> GetAllianceWoaRankingsAsync(int allianceId,
        CancellationToken ct = default);
}
