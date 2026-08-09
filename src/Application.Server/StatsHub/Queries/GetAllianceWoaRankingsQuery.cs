using Ingweland.Fog.Application.Core.Constants;
using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Application.Server.StatsHub.Factories;
using Ingweland.Fog.Dtos.Hoh.Stats;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Application.Server.StatsHub.Queries;

public record GetAllianceWoaRankingsQuery : IRequest<IReadOnlyCollection<AllianceWoaRankingDto>>, ICacheableRequest
{
    public required int AllianceId { get; init; }
    public TimeSpan? Duration => TimeSpan.FromHours(3);
    public DateTimeOffset? Expiration { get; }
}

public class GetAllianceWoaRankingsQueryHandler(
    IFogDbContext context,
    IAllianceWoaRankingDtoFactory woaRankingDtoFactory,
    ILogger<GetAllianceQueryHandler> logger)
    : IRequestHandler<GetAllianceWoaRankingsQuery, IReadOnlyCollection<AllianceWoaRankingDto>>
{
    public async Task<IReadOnlyCollection<AllianceWoaRankingDto>> Handle(GetAllianceWoaRankingsQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting alliance: {AllianceId}", request.AllianceId);
        var existingAlliance = await context.Alliances
            .Include(x =>
                x.WoaRankings.OrderByDescending(y => y.InGameEventId).Take(FogConstants.MAX_DISPLAYED_WOA_EVENTS))
            .FirstOrDefaultAsync(x => x.Id == request.AllianceId, cancellationToken);
        if (existingAlliance == null)
        {
            logger.LogInformation("Alliance with ID {AllianceId} not found", request.AllianceId);
            return [];
        }

        if (existingAlliance.WoaRankings.Count == 0)
        {
            return [];
        }

        var eventIds = existingAlliance.WoaRankings.Select(x => x.InGameEventId).ToHashSet();
        var events = await context.InGameEvents.Where(x => eventIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var divisionIds = existingAlliance.WoaRankings.Select(x => x.DivisionId).ToHashSet();
        var divisionVictoryPoints = (await context.AllianceWoaRankings
                .Where(x => divisionIds.Contains(x.DivisionId))
                .GroupBy(x => x.DivisionId)
                .Select(g => new
                    {DivisionId = g.Key, VictoryPoints = g.Sum(y => (long) y.VictoryPoints)})
                .ToListAsync(cancellationToken))
            .ToDictionary(x => x.DivisionId, x => x.VictoryPoints);

        return existingAlliance.WoaRankings.Where(x => events.ContainsKey(x.InGameEventId))
            .Select(x =>
            {
                var totalVictoryPoints = divisionVictoryPoints.GetValueOrDefault(x.DivisionId);
                return woaRankingDtoFactory.Create(x, events[x.InGameEventId], x.ExpectedVictoryPointsShare,
                    totalVictoryPoints > 0 ? (double) x.VictoryPoints / totalVictoryPoints : null);
            })
            .ToList();
    }
}
