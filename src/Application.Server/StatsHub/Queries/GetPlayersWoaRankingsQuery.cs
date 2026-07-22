using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ingweland.Fog.Application.Core.Constants;
using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Dtos.Hoh.Stats;
using Ingweland.Fog.Models.Fog;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Fog.Enums;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.Shared.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ingweland.Fog.Application.Server.StatsHub.Queries;

public record GetPlayersWoaRankingsQuery : IRequest<PaginatedList<PlayerDto>>, ICacheableRequest
{
    public int PageSize { get; init; }
    public int StartIndex { get; init; }
    public required WoaPlayerStatsCategory StatsCategory { get; init; }
    public required string WorldId { get; init; }
    public TimeSpan? Duration => TimeSpan.FromHours(1);
    public DateTimeOffset? Expiration { get; }
}

public class GetPlayersWoaRankingsQueryHandler(IFogDbContext context, IMapper mapper)
    : IRequestHandler<GetPlayersWoaRankingsQuery, PaginatedList<PlayerDto>>
{
    public async Task<PaginatedList<PlayerDto>> Handle(GetPlayersWoaRankingsQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var latestWoaEvent = await context.InGameEvents.FirstOrDefaultAsync(
            x => x.DefinitionId == EventDefinitionId.WoAEvent && x.WorldId == request.WorldId && x.StartAt <= now &&
                x.EndAt >= now, cancellationToken);
        if (latestWoaEvent == null)
        {
            latestWoaEvent = await context.InGameEvents
                .Where(x => x.DefinitionId == EventDefinitionId.WoAEvent && x.WorldId == request.WorldId &&
                    x.StartAt <= now).OrderByDescending(x => x.StartAt).FirstOrDefaultAsync(cancellationToken);

            if (latestWoaEvent == null)
            {
                return PaginatedList<PlayerDto>.Empty;
            }
        }

// TODO implement validator instead
        var pageSize = Math.Min(request.PageSize, FogConstants.MAX_PLAYERS_WOA_RANKINGS);
        var topStatsQuery = context.WoaPlayerStats
            .Where(x => x.InGameEventId == latestWoaEvent.Id);

        var total = Math.Min(await topStatsQuery.CountAsync(cancellationToken),
            FogConstants.MAX_PLAYERS_WOA_RANKINGS);
        if (total == 0)
        {
            return PaginatedList<PlayerDto>.Empty;
        }

        Expression<Func<WoaPlayerStatsEntity, int>> pointsSelector = request.StatsCategory switch
        {
            WoaPlayerStatsCategory.ContributionPoints => x => x.ContributionPoints,
            WoaPlayerStatsCategory.HealingDone => x => x.HealingDone,
            WoaPlayerStatsCategory.WonAttacks => x => x.WonAttacks,
            WoaPlayerStatsCategory.RepairsStarted => x => x.RepairsStarted,
            WoaPlayerStatsCategory.WonDefenses => x => x.WonDefenses,
            _ => x => x.VictoryPoints,
        };
        var topStats = await topStatsQuery
            .OrderByDescending(pointsSelector)
            .Skip(request.StartIndex)
            .Take(pageSize)
            .ToDictionaryAsync(x => x.PlayerId, cancellationToken);

        var playerIds = topStats.Select(x => x.Key).ToHashSet();
        var players = await context.Players
            .Where(x => playerIds.Contains(x.Id))
            .ProjectTo<PlayerDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        var today = now.ToDateOnly();
        var compiledPointsSelector = pointsSelector.Compile();
        players.ForEach(x =>
        {
            x.RankingPoints = compiledPointsSelector(topStats[x.Id]);
            x.UpdatedAt = today;
        });
        return new PaginatedList<PlayerDto>(
            players
                .OrderByDescending(x => x.RankingPoints)
                .Select((x, i) =>
                {
                    x.Rank = request.StartIndex + i + 1;
                    return x;
                })
                .ToList(),
            request.StartIndex, total);
    }
}
