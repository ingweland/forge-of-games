using Ingweland.Fog.Application.Core.Constants;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Server.PlayerCity.Queries;
using Ingweland.Fog.Application.Server.StatsHub.Queries;
using Ingweland.Fog.Application.Server.StatsHub.Queries.Tops;
using Ingweland.Fog.Dtos.Hoh.PlayerCity;
using Ingweland.Fog.Dtos.Hoh.Stats;
using Ingweland.Fog.Models.Fog;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Fog.Enums;
using Ingweland.Fog.Models.Hoh.Enums;
using MediatR;

namespace Ingweland.Fog.Application.Server.Services.Hoh;

public class StatsHubService(ISender sender) : IStatsHubService
{
    public Task<PaginatedList<PlayerDto>> GetPlayersAsync(string worldId, int startIndex,
        int pageSize = FogConstants.DEFAULT_STATS_PAGE_SIZE, string? name = null, CancellationToken ct = default)
    {
        var query = new GetPlayersWithPaginationQuery
        {
            StartIndex = startIndex, PageSize = pageSize, WorldId = worldId, Name = name,
        };
        return sender.Send(query, ct);
    }

    public Task<IReadOnlyCollection<WoaPlayerStatsDto>> GetWoaPlayerStatsAsync(int playerId,
        CancellationToken ct = default)
    {
        var query = new GetWoaPlayerStatsQuery
        {
            PlayerId = playerId,
        };
        return sender.Send(query, ct);
    }

    public Task<IReadOnlyCollection<PlayerDto>> GetTopPlayersAsync(string worldId, CancellationToken ct = default)
    {
        var query = new GetTopPlayersQuery
        {
            WorldId = worldId,
        };
        return sender.Send(query, ct);
    }

    public Task<IReadOnlyCollection<AllianceDto>> GetTopAlliancesAsync(string worldId, CancellationToken ct = default)
    {
        var query = new GetTopAlliancesQuery
        {
            WorldId = worldId,
        };
        return sender.Send(query, ct);
    }

    public Task<AllianceProfileDto?> GetAllianceAsync(int allianceId, CancellationToken ct = default)
    {
        var query = new GetAllianceQuery
        {
            AllianceId = allianceId,
        };
        return sender.Send(query, ct);
    }

    public Task<IReadOnlyCollection<AllianceAthRankingDto>> GetAllianceAthRankingsAsync(int allianceId,
        CancellationToken ct = default)
    {
        var query = new GetAllianceAthRankingsQuery
        {
            AllianceId = allianceId,
        };
        return sender.Send(query, ct);
    }

    public Task<IReadOnlyCollection<PlayerAthRankingDto>> GetPlayerAthRankingsAsync(int playerId,
        CancellationToken ct = default)
    {
        var query = new GetPlayerAthRankingsQuery
        {
            PlayerId = playerId,
        };
        return sender.Send(query, ct);
    }

    public Task<PaginatedList<AllianceDto>> GetAlliancesAsync(string worldId, int startIndex,
        int pageSize = FogConstants.DEFAULT_STATS_PAGE_SIZE,
        string? name = null, CancellationToken ct = default)
    {
        var query = new GetAlliancesWithPaginationQuery
        {
            StartIndex = startIndex, PageSize = pageSize, WorldId = worldId, Name = name,
        };
        return sender.Send(query, ct);
    }

    public Task<HohCity?> GetPlayerEventCityAsync(int playerId, CancellationToken ct = default)
    {
        var query = new GetPlayerEventCityQuery(playerId);
        return sender.Send(query, ct);
    }

    public Task<PaginatedList<PvpBattleDto>> GetPlayerBattlesAsync(int playerId, int startIndex = 0,
        int count = FogConstants.DEFAULT_STATS_PAGE_SIZE, CancellationToken ct = default)
    {
        var query = new GetPlayerBattlesQuery
        {
            PlayerId = playerId,
            StartIndex = startIndex,
            Count = count,
        };
        return sender.Send(query, ct);
    }

    public Task<PlayerDto?> GetPlayerAsync(int playerId, CancellationToken ct = default)
    {
        var query = new GetPlayerQuery(playerId);
        return sender.Send(query, ct);
    }

    public Task<PlayerProfileDto?> GetPlayerProfileAsync(int playerId, CancellationToken ct = default)
    {
        var query = new GetPlayerProfileQuery
        {
            PlayerId = playerId,
        };
        return sender.Send(query, ct);
    }

    public Task<HohCity?> GetPlayerCityAsync(int playerId, DateOnly? date = null, CancellationToken ct = default)
    {
        var query = new GetPlayerCityQuery(playerId, date);
        return sender.Send(query, ct);
    }

    public Task<IReadOnlyCollection<string>> GetTopHeroesAsync(HeroInsightsMode mode, string? ageId = null,
        int? fromLevel = null, int? toLevel = null, CancellationToken ct = default)
    {
        return sender.Send(new GetTopHeroesQuery
        {
            AgeId = ageId,
            FromLevel = fromLevel,
            ToLevel = toLevel,
            Mode = mode,
        }, ct);
    }

    public Task<IReadOnlyCollection<PvpRankingDto>> GetPlayerPvpRankingsAsync(int playerId,
        CancellationToken ct = default)
    {
        var query = new GetPlayerPvpRankingsQuery
        {
            PlayerId = playerId,
        };
        return sender.Send(query, ct);
    }

    public Task<PaginatedList<AllianceDto>> GetAlliancesAthRankingsAsync(string worldId, int startIndex = 0,
        int pageSize = FogConstants.DEFAULT_STATS_PAGE_SIZE, TreasureHuntLeague league = TreasureHuntLeague.Overlord,
        CancellationToken ct = default)
    {
        var query = new GetAlliancesAthRankingsQuery
        {
            WorldId = worldId,
            League = league,
            StartIndex = startIndex,
            PageSize = pageSize,
        };
        return sender.Send(query, ct);
    }

    public Task<PaginatedList<PlayerDto>> GetEventCityRankingsAsync(string worldId,
        CancellationToken ct = default)
    {
        var query = new GetEventCityRankingsQuery
        {
            WorldId = worldId,
        };
        return sender.Send(query, ct);
    }

    public Task<PlayerCityPropertiesDto?> GetPlayerProductionCapacityAsync(int playerId,
        CancellationToken ct = default)
    {
        var query = new GetPlayerCityPropertiesQuery(playerId);
        return sender.Send(query, ct);
    }

    public Task<IReadOnlyCollection<AllianceWoaRankingDto>> GetAllianceWoaRankingsAsync(int allianceId,
        CancellationToken ct = default)
    {
        var query = new GetAllianceWoaRankingsQuery
        {
            AllianceId = allianceId,
        };
        return sender.Send(query, ct);
    }

    public Task<IReadOnlyCollection<StatsTimedIntValue>> GetAllianceRankingsAsync(int allianceId,
        CancellationToken ct = default)
    {
        var query = new GetAllianceRankingsQuery
        {
            AllianceId = allianceId,
        };
        return sender.Send(query, ct);
    }

    public Task<IReadOnlyCollection<StatsTimedIntValue>> GetPlayerRankingsAsync(int playerId,
        CancellationToken ct = default)
    {
        var query = new GetPlayerRankingsQuery
        {
            PlayerId = playerId,
        };
        return sender.Send(query, ct);
    }
}
