using AutoMapper;
using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.Providers.Interfaces;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Client.Web.ViewModels;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Models.Fog.Enums;
using Ingweland.Fog.Models.Hoh.Enums;
using Microsoft.Extensions.Localization;

namespace Ingweland.Fog.Application.Client.Web.Services.Hoh;

public class CommonUiService : ICommonUiService
{
    private readonly IAssetUrlProvider _assetUrlProvider;
    private readonly ICommonService _commonService;
    private readonly Lazy<Task<IReadOnlyDictionary<string, AgeViewModel>>> _lazyAges;
    private readonly Lazy<Task<IReadOnlyDictionary<PvpTier, PvpTierDto>>> _lazyPvpTiers;

    private readonly Lazy<Task<IReadOnlyDictionary<TreasureHuntLeague, TreasureHuntLeagueDto>>>
        _lazyTreasureHuntLeagues;

    private readonly Lazy<IReadOnlyCollection<WoaPointsCategoryViewModel>> _lazyWoaPointsCategories;

    private readonly Lazy<Task<IReadOnlyDictionary<WoaTier, WoaTierDto>>> _lazyWoaTiers;
    private readonly IStringLocalizer<FogResource> _loc;

    private readonly IMapper _mapper;

    public CommonUiService(IMapper mapper, ICommonService commonService, IStringLocalizer<FogResource> loc,
        IAssetUrlProvider assetUrlProvider)
    {
        _mapper = mapper;
        _commonService = commonService;
        _loc = loc;
        _assetUrlProvider = assetUrlProvider;
        _lazyAges = new Lazy<Task<IReadOnlyDictionary<string, AgeViewModel>>>(InitializeAges, true);
        _lazyPvpTiers = new Lazy<Task<IReadOnlyDictionary<PvpTier, PvpTierDto>>>(InitializePvpTiers, true);
        _lazyTreasureHuntLeagues =
            new Lazy<Task<IReadOnlyDictionary<TreasureHuntLeague, TreasureHuntLeagueDto>>>(
                InitializeTreasureHuntLeagues, true);
        _lazyWoaTiers =
            new Lazy<Task<IReadOnlyDictionary<WoaTier, WoaTierDto>>>(
                InitializeWoaTiers, true);
        _lazyWoaPointsCategories =
            new Lazy<IReadOnlyCollection<WoaPointsCategoryViewModel>>(InitializeWoaPointCategories, true);
    }

    public Task<IReadOnlyDictionary<string, AgeViewModel>> GetAgesAsync()
    {
        return _lazyAges.Value;
    }

    public async Task<AgeViewModel?> GetAgeAsync(string ageId)
    {
        var ages = await _lazyAges.Value;
        return ages.GetValueOrDefault(ageId);
    }

    public Task<IReadOnlyDictionary<PvpTier, PvpTierDto>> GetPvpTiersAsync()
    {
        return _lazyPvpTiers.Value;
    }

    public Task<IReadOnlyDictionary<TreasureHuntLeague, TreasureHuntLeagueDto>> GetTreasureHuntLeaguesAsync()
    {
        return _lazyTreasureHuntLeagues.Value;
    }

    public Task<IReadOnlyDictionary<WoaTier, WoaTierDto>> GetWoaTiersAsync()
    {
        return _lazyWoaTiers.Value;
    }

    public IReadOnlyCollection<WoaPointsCategoryViewModel> GetWoaPointsCategories()
    {
        return _lazyWoaPointsCategories.Value;
    }

    private IReadOnlyCollection<WoaPointsCategoryViewModel> InitializeWoaPointCategories()
    {
        return
        [
            new WoaPointsCategoryViewModel
            {
                Category = WoaPointsCategory.Atlantis,
                Name = _loc[FogResource.WoaPointsCategory_Atlantis],
                IconUrl = _assetUrlProvider.GetHohIconUrl("icon_flat_atlantis_rating"),
            },
            new WoaPointsCategoryViewModel
            {
                Category = WoaPointsCategory.Victory,
                Name = _loc[FogResource.WoaPointsCategory_Victory],
                IconUrl = _assetUrlProvider.GetHohIconUrl("woa_victory_points"),
            },
        ];
    }

    private async Task<IReadOnlyDictionary<string, AgeViewModel>> InitializeAges()
    {
        var ages = await _commonService.GetAgesAsync();
        return _mapper.Map<IEnumerable<AgeViewModel>>(ages.OrderBy(x => x.Index)).ToDictionary(a => a.Id);
    }

    private async Task<IReadOnlyDictionary<PvpTier, PvpTierDto>> InitializePvpTiers()
    {
        var tiers = await _commonService.GetPvpTiersAsync();
        return tiers.ToDictionary(x => x.Tier);
    }

    private async Task<IReadOnlyDictionary<TreasureHuntLeague, TreasureHuntLeagueDto>> InitializeTreasureHuntLeagues()
    {
        var tiers = await _commonService.GetTreasureHuntLeaguesAsync();
        return tiers.ToDictionary(x => x.League);
    }

    private async Task<IReadOnlyDictionary<WoaTier, WoaTierDto>> InitializeWoaTiers()
    {
        var tiers = await _commonService.GetWoaTiersAsync();
        return tiers.ToDictionary(x => x.Tier);
    }
}
