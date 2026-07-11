using AutoMapper;
using Ingweland.Fog.Application.Core.Factories.Interfaces;
using Ingweland.Fog.Application.Core.Interfaces;
using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Core.Services.Hoh;

public class CommonService(
    IHohCoreDataRepository hohCoreDataRepository,
    IResourceDtoFactory resourceDtoFactory,
    IMapper mapper,
    IHohDataCache dataCache,
    IHohGameLocalizationService localizationService,
    IHohDataCacheKeyFactory cacheKeyFactory) : ICommonService
{
    public Task<IReadOnlyCollection<AgeDto>> GetAgesAsync()
    {
        var version = hohCoreDataRepository.Version;
        return dataCache.GetOrAddAsync(cacheKeyFactory.HohAges(version), async () =>
            {
                var ages = await hohCoreDataRepository.GetAges();
                return mapper.Map<IReadOnlyCollection<AgeDto>>(ages);
            },
            version);
    }

    public Task<IReadOnlyCollection<ResourceDto>> GetResourcesAsync()
    {
        var version = hohCoreDataRepository.Version;
        return dataCache.GetOrAddAsync(cacheKeyFactory.HohResources(version), async () =>
            {
                var resources = await hohCoreDataRepository.GetResources();
                return (IReadOnlyCollection<ResourceDto>) resources.Select(x => resourceDtoFactory.Create(x, x.Age))
                    .ToList();
            },
            version);
    }

    public Task<IReadOnlyCollection<PvpTierDto>> GetPvpTiersAsync()
    {
        var version = hohCoreDataRepository.Version;

        var result = dataCache.GetOrAdd(cacheKeyFactory.PvpTiers(version), IReadOnlyCollection<PvpTierDto> () =>
            {
                return Enum.GetValues<PvpTier>().Select(x => new PvpTierDto
                    {
                        Tier = x,
                        Name = localizationService.GetPvpTierName(x),
                    })
                    .OrderBy(x => x.Tier).ToList();
            },
            version);

        return Task.FromResult(result);
    }

    public Task<IReadOnlyCollection<TreasureHuntLeagueDto>> GetTreasureHuntLeaguesAsync()
    {
        var version = hohCoreDataRepository.Version;

        var result = dataCache.GetOrAdd(cacheKeyFactory.TreasureHuntLeagues(version),
            IReadOnlyCollection<TreasureHuntLeagueDto> () =>
            {
                return Enum.GetValues<TreasureHuntLeague>().Select(x => new TreasureHuntLeagueDto
                    {
                        League = x,
                        Name = localizationService.GetTreasureHuntLeagueName(x),
                    })
                    .OrderBy(x => x.League).ToList();
            },
            version);

        return Task.FromResult(result);
    }

    public Task<IReadOnlyCollection<WoaTierDto>> GetWoaTiersAsync()
    {
        var version = hohCoreDataRepository.Version;

        var result = dataCache.GetOrAdd(cacheKeyFactory.WoaTiers(version),
            IReadOnlyCollection<WoaTierDto> () =>
            {
                return Enum.GetValues<WoaTier>().Select(x => new WoaTierDto
                    {
                        Tier = x,
                        Name = localizationService.GetWoaTierName(x),
                    })
                    .OrderBy(x => x.Tier).ToList();
            },
            version);

        return Task.FromResult(result);
    }
}
