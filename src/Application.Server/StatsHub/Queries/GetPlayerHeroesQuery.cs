using Ingweland.Fog.Application.Server.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Application.Server.StatsHub.Queries;

public record GetPlayerHeroesQuery : IRequest<IReadOnlyCollection<string>>, ICacheableRequest
{
    public required int PlayerId { get; init; }
    public TimeSpan? Duration => TimeSpan.FromHours(12);
    public DateTimeOffset? Expiration { get; }
}

public class GetPlayerHeroesQueryHandler(
    IFogDbContext context,
    ILogger<GetWoaPlayerStatsQueryHandler> logger)
    : IRequestHandler<GetPlayerHeroesQuery, IReadOnlyCollection<string>>
{
    public async Task<IReadOnlyCollection<string>> Handle(GetPlayerHeroesQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting player: {PlayerId}", request.PlayerId);
        var existingPlayer = await context.Players
            .Include(x => x.Heroes)
            .FirstOrDefaultAsync(x => x.Id == request.PlayerId, cancellationToken);
        if (existingPlayer == null)
        {
            logger.LogInformation("Player with ID {PlayerId} not found", request.PlayerId);
            return [];
        }

        return existingPlayer.Heroes.Select(x => x.UnitId).ToList();
    }
}
