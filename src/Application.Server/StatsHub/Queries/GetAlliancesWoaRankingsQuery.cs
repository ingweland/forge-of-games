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

public record GetAlliancesWoaRankingsQuery : IRequest<PaginatedList<AllianceDto>>, ICacheableRequest
{
    public int PageSize { get; init; }
    public required WoaPointsCategory PointsCategory { get; init; }
    public int StartIndex { get; init; }
    public required string WorldId { get; init; }
    public TimeSpan? Duration => TimeSpan.FromHours(1);
    public DateTimeOffset? Expiration { get; }
}

public class GetAlliancesWoaRankingsQueryHandler(IFogDbContext context, IMapper mapper)
    : IRequestHandler<GetAlliancesWoaRankingsQuery, PaginatedList<AllianceDto>>
{
    public async Task<PaginatedList<AllianceDto>> Handle(GetAlliancesWoaRankingsQuery request,
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
                return PaginatedList<AllianceDto>.Empty;
            }
        }

// TODO implement validator instead
        var pageSize = Math.Min(request.PageSize, FogConstants.MAX_ALLIANCES_WOA_RANKINGS);
        var topRankingsQuery = context.AllianceWoaRankings
            .Where(x => x.InGameEventId == latestWoaEvent.Id);

        var total = Math.Min(await topRankingsQuery.CountAsync(cancellationToken),
            FogConstants.MAX_ALLIANCES_ATH_RANKINGS);
        if (total == 0)
        {
            return PaginatedList<AllianceDto>.Empty;
        }

        Expression<Func<AllianceWoaRanking, int>> pointsSelector = request.PointsCategory switch
        {
            WoaPointsCategory.Victory => x => x.VictoryPoints,
            _ => x => x.EloRating,
        };
        var topRankings = await topRankingsQuery
            .OrderByDescending(pointsSelector)
            .Skip(request.StartIndex)
            .Take(pageSize)
            .ToDictionaryAsync(x => x.AllianceId, cancellationToken);

        var allianceIds = topRankings.Select(x => x.Key).ToHashSet();
        var alliances = await context.Alliances
            .Where(x => allianceIds.Contains(x.Id))
            .ProjectTo<AllianceDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        var today = now.ToDateOnly();
        var compiledPointsSelector = pointsSelector.Compile();
        alliances.ForEach(x =>
        {
            x.RankingPoints = compiledPointsSelector(topRankings[x.Id]);
            x.UpdatedAt = today;
        });
        return new PaginatedList<AllianceDto>(
            alliances
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
