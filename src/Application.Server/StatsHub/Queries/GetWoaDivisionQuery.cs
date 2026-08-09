using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Application.Server.StatsHub.Factories;
using Ingweland.Fog.Dtos.Hoh.Stats;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Application.Server.StatsHub.Queries;

public record GetWoaDivisionQuery : IRequest<WoaDivisionDto?>, ICacheableRequest
{
    public required int DivisionId { get; init; }
    public TimeSpan? Duration => TimeSpan.FromHours(1);
    public DateTimeOffset? Expiration { get; }
}

public class GetWoaDivisionQueryHandler(
    IFogDbContext context,
    IMapper mapper,
    IAllianceWoaRankingDtoFactory woaRankingDtoFactory,
    ILogger<GetWoaDivisionQueryHandler> logger)
    : IRequestHandler<GetWoaDivisionQuery, WoaDivisionDto?>
{
    public async Task<WoaDivisionDto?> Handle(GetWoaDivisionQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting woa division: {DivisionId}", request.DivisionId);
        var rankings = await context.AllianceWoaRankings.AsNoTracking()
            .Where(x => x.DivisionId == request.DivisionId)
            .ToListAsync(cancellationToken);
        if (rankings.Count == 0)
        {
            logger.LogInformation("Woa division with ID {DivisionId} not found", request.DivisionId);
            return null;
        }

        var inGameEventId = rankings[0].InGameEventId;
        var inGameEvent =
            await context.InGameEvents.FirstOrDefaultAsync(x => x.Id == inGameEventId, cancellationToken);
        if (inGameEvent == null)
        {
            logger.LogInformation("In-game event with ID {EventId} not found", inGameEventId);
            return null;
        }

        var allianceIds = rankings.Select(x => x.AllianceId).ToHashSet();
        var alliances = await context.Alliances
            .Where(x => allianceIds.Contains(x.Id))
            .ProjectTo<AllianceDto>(mapper.ConfigurationProvider)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var totalVictoryPoints = rankings.Sum(x => (long) x.VictoryPoints);

        var items = new List<WoaDivisionAllianceDto>(rankings.Count);
        foreach (var ranking in rankings.OrderByDescending(x => x.VictoryPoints))
        {
            if (!alliances.TryGetValue(ranking.AllianceId, out var alliance))
            {
                logger.LogWarning("Alliance with ID {AllianceId} not found", ranking.AllianceId);
                continue;
            }

            items.Add(new WoaDivisionAllianceDto
            {
                Alliance = alliance,
                Ranking = woaRankingDtoFactory.Create(ranking, inGameEvent, ranking.ExpectedVictoryPointsShare,
                    totalVictoryPoints > 0 ? (double) ranking.VictoryPoints / totalVictoryPoints : null),
            });
        }

        return new WoaDivisionDto
        {
            DivisionId = request.DivisionId,
            StartedAt = inGameEvent.StartAt,
            EndedAt = inGameEvent.EndAt,
            Alliances = items,
        };
    }
}
