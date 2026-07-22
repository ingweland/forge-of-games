using Ingweland.Fog.Application.Core.Constants;
using Ingweland.Fog.Application.Core.Helpers;
using Ingweland.Fog.Dtos.Hoh.PlayerCity;
using Ingweland.Fog.Dtos.Hoh.Stats;
using Ingweland.Fog.Models.Fog;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Fog.Enums;
using Ingweland.Fog.Models.Hoh.Enums;
using Refit;

namespace Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;

public interface IStatsHubService
{
    [Get(FogUrlBuilder.ApiRoutes.PLAYER_PROFILE_TEMPLATE_REFIT)]
    Task<PlayerProfileDto?> GetPlayerProfileAsync(int playerId, CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.PLAYER_CITY_TEMPLATE_REFIT)]
    Task<HohCity?> GetPlayerCityAsync(int playerId, DateOnly? date = null, CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.PLAYER_EVENT_CITY_TEMPLATE_REFIT)]
    Task<HohCity?> GetPlayerEventCityAsync(int playerId, CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.PLAYER_BATTLES_TEMPLATE_REFIT)]
    Task<PaginatedList<PvpBattleDto>> GetPlayerBattlesAsync(int playerId, [Query] int startIndex = 0,
        [Query] int count = FogConstants.DEFAULT_STATS_PAGE_SIZE, CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.PLAYER_TEMPLATE_REFIT)]
    Task<PlayerDto?> GetPlayerAsync(int playerId, CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.PLAYERS_TEMPLATE)]
    Task<PaginatedList<PlayerDto>> GetPlayersAsync(string worldId, [Query] int startIndex = 0,
        [Query] int pageSize = FogConstants.DEFAULT_STATS_PAGE_SIZE, [Query] string? name = null,
        CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.TOP_PLAYERS_TEMPLATE)]
    Task<IReadOnlyCollection<PlayerDto>> GetTopPlayersAsync(string worldId, CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.TOP_ALLIANCES_TEMPLATE)]
    Task<IReadOnlyCollection<AllianceDto>> GetTopAlliancesAsync(string worldId, CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.ALLIANCE_TEMPLATE_REFIT)]
    Task<AllianceProfileDto?> GetAllianceAsync(int allianceId, CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.ALLIANCE_ATH_RANKINGS_TEMPLATE_REFIT)]
    Task<IReadOnlyCollection<AllianceAthRankingDto>> GetAllianceAthRankingsAsync(int allianceId,
        CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.PLAYER_ATH_RANKINGS_TEMPLATE_REFIT)]
    Task<IReadOnlyCollection<PlayerAthRankingDto>> GetPlayerAthRankingsAsync(int playerId,
        CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.ALLIANCE_RANKINGS_TEMPLATE_REFIT)]
    Task<IReadOnlyCollection<StatsTimedIntValue>> GetAllianceRankingsAsync(int allianceId,
        CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.PLAYER_RANKINGS_TEMPLATE_REFIT)]
    Task<IReadOnlyCollection<StatsTimedIntValue>> GetPlayerRankingsAsync(int playerId,
        CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.ALLIANCES_TEMPLATE)]
    Task<PaginatedList<AllianceDto>> GetAlliancesAsync(string worldId, [Query] int startIndex = 0,
        [Query] int pageSize = FogConstants.DEFAULT_STATS_PAGE_SIZE, [Query] string? name = null,
        CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.TOP_HEROES_PATH)]
    Task<IReadOnlyCollection<string>> GetTopHeroesAsync([Query] HeroInsightsMode mode, [Query] string? ageId = null,
        [Query] int? fromLevel = null, [Query] int? toLevel = null, CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.PLAYER_PVP_RANKINGS_TEMPLATE_REFIT)]
    Task<IReadOnlyCollection<PvpRankingDto>> GetPlayerPvpRankingsAsync(int playerId,
        CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.ALLIANCES_ATH_RANKINGS_TEMPLATE)]
    Task<PaginatedList<AllianceDto>> GetAlliancesAthRankingsAsync(string worldId, [Query] int startIndex = 0,
        [Query] int pageSize = FogConstants.DEFAULT_STATS_PAGE_SIZE,
        [Query] TreasureHuntLeague league = TreasureHuntLeague.Overlord,
        CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.WORLD_EVENT_CITY_TEMPLATE)]
    Task<PaginatedList<PlayerDto>> GetEventCityRankingsAsync(string worldId, CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.PLAYER_PRODUCTION_CAPACITY_TEMPLATE_REFIT)]
    Task<PlayerCityPropertiesDto?> GetPlayerProductionCapacityAsync(int playerId, CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.ALLIANCE_WOA_RANKINGS_TEMPLATE_REFIT)]
    Task<IReadOnlyCollection<AllianceWoaRankingDto>> GetAllianceWoaRankingsAsync(int allianceId,
        CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.WOA_PLAYER_STATS_TEMPLATE_REFIT)]
    Task<IReadOnlyCollection<WoaPlayerStatsDto>> GetWoaPlayerStatsAsync(int playerId, CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.PLAYER_HEROES_TEMPLATE_REFIT)]
    Task<IReadOnlyCollection<string>> GetPlayerHeroesAsync(int playerId, CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.ALLIANCES_WOA_RANKINGS_TEMPLATE)]
    Task<PaginatedList<AllianceDto>> GetAlliancesWoaRankingsAsync(string worldId, [Query] int startIndex = 0,
        [Query] int pageSize = FogConstants.DEFAULT_STATS_PAGE_SIZE,
        [Query] WoaPointsCategory pointsCategory = WoaPointsCategory.Atlantis,
        CancellationToken ct = default);

    [Get(FogUrlBuilder.ApiRoutes.PLAYERS_WOA_RANKINGS_TEMPLATE)]
    Task<PaginatedList<PlayerDto>> GetPlayersWoaRankingsAsync(string worldId, [Query] int startIndex = 0,
        [Query] int pageSize = FogConstants.DEFAULT_STATS_PAGE_SIZE,
        [Query] WoaPlayerStatsCategory statsCategory = WoaPlayerStatsCategory.VictoryPoints,
        CancellationToken ct = default);
}
