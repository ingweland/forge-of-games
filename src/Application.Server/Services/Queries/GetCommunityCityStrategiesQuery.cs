using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using FluentResults.Extensions;
using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Dtos.Hoh;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ingweland.Fog.Application.Server.Services.Queries;

public record GetCommunityCityStrategiesQuery : IRequest<Result<IReadOnlyCollection<CommunityCityStrategyDto>>>,
    ICacheableRequest
{
    public TimeSpan? Duration => TimeSpan.FromHours(3);
    public DateTimeOffset? Expiration { get; }
}

public class GetCommunityCityStrategiesQueryHandler(IFogDbContext context, IMapper mapper)
    : IRequestHandler<GetCommunityCityStrategiesQuery, Result<IReadOnlyCollection<CommunityCityStrategyDto>>>
{
    public Task<Result<IReadOnlyCollection<CommunityCityStrategyDto>>> Handle(
        GetCommunityCityStrategiesQuery request, CancellationToken cancellationToken)
    {
        return Result.Try(() => context.CommunityCityStrategies
                .Where(x => x.IsEnabled)
                .ProjectTo<CommunityCityStrategyDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken))
            .Map(IReadOnlyCollection<CommunityCityStrategyDto> (x) => x);
    }
}
