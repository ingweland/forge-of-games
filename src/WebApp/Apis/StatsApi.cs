using Ingweland.Fog.Application.Core.Helpers;
using Ingweland.Fog.Application.Server.PlayerCity.Queries;
using Ingweland.Fog.Application.Server.StatsHub.Queries;
using Ingweland.Fog.Application.Server.StatsHub.Queries.Tops;
using Ingweland.Fog.Dtos.Hoh.Battle;
using Ingweland.Fog.Dtos.Hoh.PlayerCity;
using Ingweland.Fog.Dtos.Hoh.Stats;
using Ingweland.Fog.Models.Fog;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Ingweland.Fog.WebApp.Apis;

public static class StatsApi
{
    public static RouteGroupBuilder MapStatsApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api/hoh");

        api.MapGet(FogUrlBuilder.ApiRoutes.TOP_HEROES_PATH, GetTopHeroesAsync);

        api.MapGet(FogUrlBuilder.ApiRoutes.PLAYERS_TEMPLATE, GetPlayersAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.PLAYER_PROFILE_TEMPLATE, GetPlayerProfileAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.PLAYER_CITY_TEMPLATE, GetPlayerCityAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.PLAYER_EVENT_CITY_TEMPLATE, GetPlayerEventCityAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.PLAYER_TEMPLATE, GetPlayerAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.PLAYER_BATTLES_TEMPLATE, GetPlayerBattlesAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.TOP_PLAYERS_TEMPLATE, GetTopPlayersAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.PLAYER_WONDER_RANKINGS_TEMPLATE, GetWonderRankingsAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.PLAYER_ATH_RANKINGS_TEMPLATE, GetPlayerAthRankingsAsync);

        api.MapGet(FogUrlBuilder.ApiRoutes.ALLIANCES_TEMPLATE, GetAlliancesAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.ALLIANCE_TEMPLATE, GetAllianceAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.ALLIANCE_ATH_RANKINGS_TEMPLATE, GetAllianceAthRankingsAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.ALLIANCE_WOA_RANKINGS_TEMPLATE, GetAllianceWoaRankingsAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.ALLIANCE_RANKINGS_TEMPLATE, GetAllianceRankingsAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.ALLIANCES_ATH_RANKINGS_TEMPLATE, GetAlliancesAthRankingsAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.TOP_ALLIANCES_TEMPLATE, GetTopAlliancesAsync);

        api.MapGet(FogUrlBuilder.ApiRoutes.WORLD_EVENT_CITY_TEMPLATE, GetEventCityRankingsAsync);

        api.MapPost(FogUrlBuilder.ApiRoutes.BATTLE_LOG_SEARCH, SearchBattlesAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.BATTLE_STATS_TEMPLATE, GetBattleStatsAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.BATTLE_TEMPLATE, GetBattleAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.UNIT_BATTLES_TEMPLATE, GetUnitBattlesAsync);
        api.MapPost(FogUrlBuilder.ApiRoutes.PLAYER_CITY_SNAPSHOTS_SEARCH, SearchCityInspirationsAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.PLAYER_CITY_SNAPSHOT_TEMPLATE, GetPlayerCitySnapshotAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.PLAYER_PVP_RANKINGS_TEMPLATE, GetPvpRankingsAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.PLAYER_RANKINGS_TEMPLATE, GetPlayerRankingsAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.PLAYER_PRODUCTION_CAPACITY_TEMPLATE, GetPlayerProductionCapacityAsync);

        api.MapPost(FogUrlBuilder.ApiRoutes.USER_BATTLE_SEARCH, SearchUserBattlesAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.EQUIPMENT_INSIGHTS_TEMPLATE, GetEquipmentInsightsAsync);
        api.MapGet(FogUrlBuilder.ApiRoutes.RELICS_INSIGHTS_TEMPLATE, GetRelicInsightsAsync);

        return api;
    }

    private static async Task<Results<Ok<PlayerProfileDto>, NotFound, BadRequest<string>>>
        GetPlayerProfileAsync([AsParameters] StatsServices services, HttpContext context, int playerId,
            CancellationToken ct = default)
    {
        var query = new GetPlayerProfileQuery
        {
            PlayerId = playerId,
        };
        var result = await services.Mediator.Send(query, ct);
        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<string>>, BadRequest<string>>>
        GetTopHeroesAsync([AsParameters] StatsServices services, HttpContext context,
            [AsParameters] GetTopHeroesQuery query, CancellationToken ct = default)
    {
        var result = await services.Mediator.Send(query, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<HohCity>, NotFound, BadRequest<string>>>
        GetPlayerCityAsync([AsParameters] StatsServices services, HttpContext context, int playerId,
            [FromQuery] DateOnly? date = null, CancellationToken ct = default)
    {
        var query = new GetPlayerCityQuery(playerId, date);
        var result = await services.Mediator.Send(query, ct);
        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<HohCity>, NotFound, BadRequest<string>>>
        GetPlayerEventCityAsync([AsParameters] StatsServices services, HttpContext context, int playerId,
            CancellationToken ct = default)
    {
        var query = new GetPlayerEventCityQuery(playerId);
        var result = await services.Mediator.Send(query, ct);
        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<BattleStatsDto>, NotFound>>
        GetBattleStatsAsync([AsParameters] StatsServices services, HttpContext context, int battleStatsId,
            CancellationToken ct = default)
    {
        var result = await services.BattleService.GetBattleStatsAsync(battleStatsId, ct);
        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<BattleDto>, NotFound>>
        GetBattleAsync([AsParameters] StatsServices services, HttpContext context, int battleId,
            CancellationToken ct = default)
    {
        var result = await services.BattleService.GetBattleAsync(battleId, ct);
        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<HohCity>, NotFound>>
        GetPlayerCitySnapshotAsync([AsParameters] StatsServices services, HttpContext context, int snapshotId,
            CancellationToken ct)
    {
        var result = await services.CityPlannerService.GetPlayerCitySnapshotAsync(snapshotId, ct);
        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyCollection<EquipmentInsightsDto>>>
        GetEquipmentInsightsAsync([AsParameters] StatsServices services, HttpContext context, string unitId,
            CancellationToken ct)
    {
        var result = await services.EquipmentInsightsService.GetInsightsAsync(unitId, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyCollection<RelicInsightsDto>>>
        GetRelicInsightsAsync([AsParameters] StatsServices services, HttpContext context, string unitId,
            CancellationToken ct)
    {
        var result = await services.RelicService.GetInsightsAsync(unitId, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<PlayerDto>, NotFound>>
        GetPlayerAsync([AsParameters] StatsServices services, HttpContext context, int playerId,
            CancellationToken ct)
    {
        var result = await services.StatsHubService.GetPlayerAsync(playerId, ct);
        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<PaginatedList<PvpBattleDto>>>
        GetPlayerBattlesAsync([AsParameters] StatsServices services, HttpContext context,
            [AsParameters] GetPlayerBattlesQuery query,
            CancellationToken ct)
    {
        var result = await services.Mediator.Send(query, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyCollection<UnitBattleDto>>>
        GetUnitBattlesAsync([AsParameters] StatsServices services, HttpContext context, string unitId,
            BattleType battleType, CancellationToken ct = default)
    {
        var result = await services.BattleService.GetUnitBattlesAsync(unitId, battleType, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<PaginatedList<PlayerDto>>, BadRequest<string>>>
        GetPlayersAsync([AsParameters] StatsServices services, HttpContext context,
            [AsParameters] GetPlayersWithPaginationQuery query, CancellationToken ct = default)
    {
        var result = await services.Mediator.Send(query, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<PlayerDto>>, BadRequest<string>>>
        GetTopPlayersAsync([AsParameters] StatsServices services, HttpContext context,
            [AsParameters] GetTopPlayersQuery query, CancellationToken ct = default)
    {
        var result = await services.Mediator.Send(query, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<PlayerCityPropertiesDto>, NotFound, BadRequest<string>>>
        GetPlayerProductionCapacityAsync([AsParameters] StatsServices services, HttpContext context,
            [AsParameters] GetPlayerCityPropertiesQuery query, CancellationToken ct = default)
    {
        var result = await services.Mediator.Send(query, ct);
        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<AllianceProfileDto>, NotFound, BadRequest<string>>>
        GetAllianceAsync([AsParameters] StatsServices services, HttpContext context, int allianceId,
            CancellationToken ct = default)
    {
        var query = new GetAllianceQuery
        {
            AllianceId = allianceId,
        };
        var result = await services.Mediator.Send(query, ct);
        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyCollection<AllianceAthRankingDto>>>
        GetAllianceAthRankingsAsync([AsParameters] StatsServices services, HttpContext context, int allianceId,
            CancellationToken ct = default)
    {
        var result = await services.StatsHubService.GetAllianceAthRankingsAsync(allianceId, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyCollection<AllianceWoaRankingDto>>>
        GetAllianceWoaRankingsAsync([AsParameters] StatsServices services, HttpContext context, int allianceId,
            CancellationToken ct = default)
    {
        var result = await services.StatsHubService.GetAllianceWoaRankingsAsync(allianceId, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyCollection<PlayerAthRankingDto>>>
        GetPlayerAthRankingsAsync([AsParameters] StatsServices services, HttpContext context, int playerId,
            CancellationToken ct = default)
    {
        var result = await services.StatsHubService.GetPlayerAthRankingsAsync(playerId, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyCollection<WonderRankingDto>>>
        GetWonderRankingsAsync([AsParameters] StatsServices services, HttpContext context, int playerId,
            CancellationToken ct = default)
    {
        var result = await services.StatsHubService.GetWonderRankingsAsync(playerId, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyCollection<StatsTimedIntValue>>>
        GetAllianceRankingsAsync([AsParameters] StatsServices services, HttpContext context, int allianceId,
            CancellationToken ct = default)
    {
        var result = await services.StatsHubService.GetAllianceRankingsAsync(allianceId, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyCollection<StatsTimedIntValue>>>
        GetPlayerRankingsAsync([AsParameters] StatsServices services, HttpContext context, int playerId,
            CancellationToken ct = default)
    {
        var result = await services.StatsHubService.GetPlayerRankingsAsync(playerId, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyCollection<PvpRankingDto>>>
        GetPvpRankingsAsync([AsParameters] StatsServices services, HttpContext context, int playerId,
            CancellationToken ct = default)
    {
        var result = await services.StatsHubService.GetPlayerPvpRankingsAsync(playerId, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<PaginatedList<AllianceDto>>, BadRequest<string>>>
        GetAlliancesAsync([AsParameters] StatsServices services, HttpContext context,
            [AsParameters] GetAlliancesWithPaginationQuery query,
            CancellationToken ct = default)
    {
        var result = await services.Mediator.Send(query, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<AllianceDto>>, BadRequest<string>>>
        GetTopAlliancesAsync([AsParameters] StatsServices services, HttpContext context,
            [AsParameters] GetTopAlliancesQuery query, CancellationToken ct = default)
    {
        var result = await services.Mediator.Send(query, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<PaginatedList<AllianceDto>>, BadRequest<string>>>
        GetAlliancesAthRankingsAsync([AsParameters] StatsServices services, HttpContext context,
            [AsParameters] GetAlliancesAthRankingsQuery query,
            CancellationToken ct = default)
    {
        var result = await services.Mediator.Send(query, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<PaginatedList<PlayerDto>>, BadRequest<string>>>
        GetEventCityRankingsAsync([AsParameters] StatsServices services, HttpContext context,
            [AsParameters] GetEventCityRankingsQuery query,
            CancellationToken ct = default)
    {
        var result = await services.Mediator.Send(query, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<BattleSearchResult>, NotFound, BadRequest<string>>>
        SearchBattlesAsync([AsParameters] StatsServices services, HttpContext context,
            [FromBody] BattleSearchRequest request, CancellationToken ct = default)
    {
        var result = await services.BattleService.SearchBattlesAsync(request, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<PaginatedList<BattleSummaryDto>>, NotFound, BadRequest<string>>>
        SearchUserBattlesAsync([AsParameters] StatsServices services, HttpContext context,
            [FromBody] UserBattleSearchRequest request, CancellationToken ct = default)
    {
        var result = await services.BattleService.SearchBattlesAsync(request, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<PlayerCitySnapshotBasicDto>>, BadRequest<string>>>
        SearchCityInspirationsAsync([AsParameters] StatsServices services, HttpContext context,
            [FromBody] CityInspirationsSearchRequest request, CancellationToken ct = default)
    {
        var result = await services.CityPlannerService.GetInspirationsAsync(request, ct);

        return TypedResults.Ok(result);
    }
}
