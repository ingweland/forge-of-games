using Ingweland.Fog.Application.Core.Constants;
using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Application.Server.StatsHub.Factories;
using Ingweland.Fog.Dtos.Hoh.Stats;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Application.Server.StatsHub.Queries;

public record GetWoaPlayerStatsQuery : IRequest<IReadOnlyCollection<WoaPlayerStatsDto>>, ICacheableRequest
{
    public required int PlayerId { get; init; }
    public TimeSpan? Duration => TimeSpan.FromHours(3);
    public DateTimeOffset? Expiration { get; }
}

public class GetWoaPlayerStatsQueryHandler(
    IFogDbContext context,
    IWoaPlayerStatsDtoFactory woaPlayerStatsDtoFactory,
    ILogger<GetWoaPlayerStatsQueryHandler> logger)
    : IRequestHandler<GetWoaPlayerStatsQuery, IReadOnlyCollection<WoaPlayerStatsDto>>
{
    public async Task<IReadOnlyCollection<WoaPlayerStatsDto>> Handle(GetWoaPlayerStatsQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting player: {PlayerId}", request.PlayerId);
        var existingPlayer = await context.Players
            .Include(x =>
                x.WoaStats.OrderByDescending(y => y.InGameEventId).Take(FogConstants.MAX_DISPLAYED_WOA_EVENTS))
            .FirstOrDefaultAsync(x => x.Id == request.PlayerId, cancellationToken);
        if (existingPlayer == null)
        {
            logger.LogInformation("Player with ID {PlayerId} not found", request.PlayerId);
            return [];
        }

        if (existingPlayer.WoaStats.Count == 0)
        {
            return [];
        }

        var eventIds = existingPlayer.WoaStats.Select(x => x.InGameEventId).ToHashSet();
        var events = await context.InGameEvents.Where(x => eventIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        return existingPlayer.WoaStats.Where(x => events.ContainsKey(x.InGameEventId))
            .Select(x => woaPlayerStatsDtoFactory.Create(x, events[x.InGameEventId]))
            .ToList();
    }
}
