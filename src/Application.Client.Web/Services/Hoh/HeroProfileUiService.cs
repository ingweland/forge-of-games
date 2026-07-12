using AutoMapper;
using Ingweland.Fog.Application.Client.Web.Caching.Interfaces;
using Ingweland.Fog.Application.Client.Web.Calculators.Interfaces;
using Ingweland.Fog.Application.Client.Web.CommandCenter.Abstractions;
using Ingweland.Fog.Application.Client.Web.Factories.Interfaces;
using Ingweland.Fog.Application.Client.Web.Models;
using Ingweland.Fog.Application.Client.Web.Providers.Interfaces;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh.Units;
using Ingweland.Fog.Application.Core.Extensions;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Dtos.Hoh.City;
using Ingweland.Fog.Dtos.Hoh.Units;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Enums;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Application.Client.Web.Services.Hoh;

public class HeroProfileUiService : UiServiceBase, IHeroProfileUiService
{
    private readonly IAssetUrlProvider _assetUrlProvider;
    private readonly IBuildingLevelRangesFactory _buildingLevelRangesFactory;
    private readonly IHohCoreDataCache _coreDataCache;
    private readonly Lazy<Task<IReadOnlyDictionary<string, HeroBasicViewModel>>> _heroList;
    private readonly IHeroProfileIdentifierFactory _heroProfileIdentifierFactory;
    private readonly IHohHeroProfileViewModelFactory _heroProfileViewModelFactory;
    private readonly IHeroProgressionCalculators _heroProgressionCalculators;
    private readonly IHohHeroProfileFactory _hohHeroProfileFactory;
    private readonly IMapper _mapper;
    private readonly IPersistenceService _persistenceService;
    private readonly IUnitService _unitService;
    private IReadOnlyDictionary<string, HashSet<string>>? _heroAbilityTagsToHeroIdsMap;

    private IReadOnlyCollection<HeroBasicDto> _heroes = [];

    public HeroProfileUiService(
        IPersistenceService persistenceService,
        IHohHeroProfileViewModelFactory heroProfileViewModelFactory,
        IUnitService unitService,
        IHohHeroProfileFactory hohHeroProfileFactory,
        IBuildingLevelRangesFactory buildingLevelRangesFactory,
        IHohCoreDataCache coreDataCache,
        IHeroProgressionCalculators heroProgressionCalculators,
        IHeroProfileIdentifierFactory heroProfileIdentifierFactory,
        IAssetUrlProvider assetUrlProvider,
        IMapper mapper,
        ILogger<HeroProfileUiService> logger) : base(logger)
    {
        _persistenceService = persistenceService;
        _heroProfileViewModelFactory = heroProfileViewModelFactory;
        _unitService = unitService;
        _hohHeroProfileFactory = hohHeroProfileFactory;
        _buildingLevelRangesFactory = buildingLevelRangesFactory;
        _coreDataCache = coreDataCache;
        _heroProgressionCalculators = heroProgressionCalculators;
        _heroProfileIdentifierFactory = heroProfileIdentifierFactory;
        _assetUrlProvider = assetUrlProvider;
        _mapper = mapper;

        _heroList = new Lazy<Task<IReadOnlyDictionary<string, HeroBasicViewModel>>>(DoGetHeroesAsync);
    }

    public async Task<IReadOnlyCollection<string>> GetHeroAbilityTagsAsync()
    {
        _ = await _heroList.Value;
        return _heroAbilityTagsToHeroIdsMap!.Keys.Order().ToList();
    }

    public async Task<IconLabelItemViewModel> CalculateAbilityCostAsync(AbilityCostRequest request)
    {
        var hero = await _coreDataCache.GetHeroAsync(request.HeroId);
        if (hero == null)
        {
            return IconLabelItemViewModel.Blank;
        }

        return new IconLabelItemViewModel
        {
            Label = _heroProgressionCalculators
                .CalculateAbilityCost(hero.Ability, request.CurrentLevel, request.TargetLevel)
                .ToString("N0"),
            IconUrl = _assetUrlProvider.GetHohIconUrl("icon_mastery_points"),
        };
    }

    public void SaveHeroProfile(HeroProfileIdentifier identifier)
    {
        Task.Run(async () => { await _persistenceService.SaveHeroProfileAsync(identifier); });
    }

    public Task<IReadOnlyCollection<HeroBasicViewModel>> GetHeroes(HeroFilterRequest request)
    {
        return GetHeroes(request, new HashSet<string>());
    }

    public async Task<IReadOnlyCollection<HeroBasicViewModel>> GetHeroes(HeroFilterRequest request,
        IReadOnlySet<string> heroIds)
    {
        var heroVms = await _heroList.Value;
        var query = _heroes.AsEnumerable();

        if (heroIds.Count > 0)
        {
            query = query.Where(x => heroIds.Contains(x.Id) || heroIds.Contains(x.UnitId));
        }

        if (request.Classes.Count > 0)
        {
            query = query.Where(x => request.Classes.Contains(x.ClassId));
        }

        if (request.UnitColors.Count > 0)
        {
            query = query.Where(x => request.UnitColors.Contains(x.UnitColor));
        }

        if (request.UnitTypes.Count > 0)
        {
            query = query.Where(x => request.UnitTypes.Contains(x.UnitType));
        }

        if (request.StarClasses.Count > 0)
        {
            query = query.Where(x => request.StarClasses.Contains(x.StarClass));
        }

        if (request.AbilityTag != null && _heroAbilityTagsToHeroIdsMap!.ContainsKey(request.AbilityTag))
        {
            query = query.Where(x => _heroAbilityTagsToHeroIdsMap![request.AbilityTag].Contains(x.Id));
        }

        return query.OrderBy(x => x.Name).Select(x => heroVms[x.Id]).ToList();
    }

    public Task<IReadOnlyCollection<HeroBasicViewModel>> GetHeroes(string searchString)
    {
        return GetHeroes(searchString, new HashSet<string>());
    }

    public async Task<IReadOnlyCollection<HeroBasicViewModel>> GetHeroes(string searchString,
        IReadOnlySet<string> heroIds)
    {
        var heroVms = await _heroList.Value;
        var query = heroVms.Where(kvp =>
            kvp.Value.Name.Contains(searchString, StringComparison.InvariantCultureIgnoreCase));
        if (heroIds.Count > 0)
        {
            query = query.Where(x => heroIds.Contains(x.Value.Id));
        }

        return query.Select(kvp => kvp.Value).OrderBy(x => x.Name).ToList();
    }

    public async Task<IReadOnlyCollection<HeroBasicViewModel>> GetHeroes()
    {
        var heroVms = await _heroList.Value;
        return heroVms.Values.ToList();
    }

    public Task<HeroDto?> GetHeroAsync(string heroId)
    {
        return _coreDataCache.GetHeroAsync(heroId);
    }

    public async Task<HeroProfileIdentifier?> GetHeroProfileIdentifierAsync(string heroId)
    {
        var hero = await _coreDataCache.GetHeroAsync(heroId);
        if (hero == null)
        {
            return null;
        }

        if (OperatingSystem.IsBrowser())
        {
            var savedProfile = await _persistenceService.GetHeroProfileAsync(heroId);
            if (savedProfile != null)
            {
                return savedProfile;
            }
        }

        var barracks = await _coreDataCache.GetBarracks(hero.Unit.Type);
        return _heroProfileIdentifierFactory.Create(hero.Id, barracks.OrderBy(x => x.Level).First().Level);
    }

    public async Task<HeroProfileViewModel?> GetHeroProfileAsync(HeroProfileIdentifier identifier)
    {
        var hero = await _coreDataCache.GetHeroAsync(identifier.HeroId);
        if (hero == null)
        {
            return null;
        }

        var barracks = await _coreDataCache.GetBarracks(hero.Unit.Type);
        var profile = _hohHeroProfileFactory.Create(identifier, hero,
            barracks.FirstOrDefault(x => x.Level == identifier.BarracksLevel));
        return _heroProfileViewModelFactory.Create(profile, hero, barracks);
    }

    public async Task<IReadOnlyCollection<IconLabelItemViewModel>> CalculateHeroProgressionCost(
        HeroProgressionCostRequest request)
    {
        var hero = await _coreDataCache.GetHeroAsync(request.HeroId);
        return _mapper.Map<IReadOnlyCollection<IconLabelItemViewModel>>(
            _heroProgressionCalculators.CalculateProgressionCost(hero!, request.CurrentLevel, request.TargetLevel));
    }

    public async Task<HeroBasicViewModel?> GetHeroBasicsAsync(string heroId)
    {
        var heroVms = await _heroList.Value;
        return heroVms.GetValueOrDefault(heroId);
    }

    private void CreateHeroAbilityTags(IReadOnlyDictionary<string, IReadOnlySet<string>> src)
    {
        var map = new Dictionary<string, HashSet<string>>();
        foreach (var kvp in src)
        {
            foreach (var tag in kvp.Value)
            {
                if (!map.TryGetValue(tag, out var heroIds))
                {
                    heroIds = new HashSet<string>();
                    map.Add(tag, heroIds);
                }

                heroIds.Add(kvp.Key);
            }
        }

        _heroAbilityTagsToHeroIdsMap = map;
    }

    private async Task<IReadOnlyDictionary<string, HeroBasicViewModel>> DoGetHeroesAsync()
    {
        _heroes = await ExecuteSafeAsync(() => _unitService.GetHeroesBasicDataAsync(), []);
        CreateHeroAbilityTags(_heroes.ToDictionary(x => x.Id, x => x.AbilityTags));
        return _mapper.Map<IReadOnlyCollection<HeroBasicViewModel>>(_heroes).ToDictionary(x => x.Id);
    }

    private BuildingLevelRange GetBarracksLevels(IReadOnlyCollection<BuildingDto> barracks, UnitType unitType)
    {
        var group = unitType.ToBuildingGroup();
        var ranges = _buildingLevelRangesFactory.Create(barracks);
        return ranges[group];
    }
}
