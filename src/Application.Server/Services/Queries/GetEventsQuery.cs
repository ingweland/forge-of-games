using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Models.Hoh.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ingweland.Fog.Application.Server.Services.Queries;

public record GetEventsQuery : IRequest<IReadOnlyCollection<InGameEventDto>>, ICacheableRequest
{
    public required EventDefinitionId EventDefinitionId { get; init; }
    public required string WorldId { get; init; }
    public TimeSpan? Duration => TimeSpan.FromHours(24);
    public DateTimeOffset? Expiration { get; }
}

public class GetEventsQueryHandler(IFogDbContext context, IMapper mapper)
    : IRequestHandler<GetEventsQuery, IReadOnlyCollection<InGameEventDto>>
{
    public async Task<IReadOnlyCollection<InGameEventDto>> Handle(GetEventsQuery request,
        CancellationToken cancellationToken)
    {
        return await context.InGameEvents
            .Where(x => x.DefinitionId == request.EventDefinitionId && x.WorldId == request.WorldId)
            .ProjectTo<InGameEventDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
