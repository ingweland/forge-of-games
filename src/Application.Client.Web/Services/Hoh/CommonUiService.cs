using AutoMapper;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.Services.Hoh;

public class CommonUiService : ICommonUiService
{
    private readonly ICommonService _commonService;
    private readonly Lazy<Task<IReadOnlyDictionary<string, AgeViewModel>>> _lazyAges;
    private readonly Lazy<Task<IReadOnlyDictionary<PvpTier, PvpTierDto>>> _lazyPvpTiers;

    private readonly Lazy<Task<IReadOnlyDictionary<TreasureHuntLeague, TreasureHuntLeagueDto>>>
        _lazyTreasureHuntLeagues;

    private readonly Lazy<Task<IReadOnlyDictionary<WoaTier, WoaTierDto>>> _lazyWoaTiers;

    private readonly IMapper _mapper;

    public CommonUiService(IMapper mapper, ICommonService commonService)
    {
        _mapper = mapper;
        _commonService = commonService;
        _lazyAges = new Lazy<Task<IReadOnlyDictionary<string, AgeViewModel>>>(InitializeAges, true);
        _lazyPvpTiers = new Lazy<Task<IReadOnlyDictionary<PvpTier, PvpTierDto>>>(InitializePvpTiers, true);
        _lazyTreasureHuntLeagues =
            new Lazy<Task<IReadOnlyDictionary<TreasureHuntLeague, TreasureHuntLeagueDto>>>(
                InitializeTreasureHuntLeagues, true);
        _lazyWoaTiers =
            new Lazy<Task<IReadOnlyDictionary<WoaTier, WoaTierDto>>>(
                InitializeWoaTiers, true);
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
