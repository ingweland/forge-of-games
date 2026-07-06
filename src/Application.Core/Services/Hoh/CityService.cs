using Ingweland.Fog.Application.Core.Extensions;
using Ingweland.Fog.Application.Core.Factories.Interfaces;
using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Dtos.Hoh.City;
using Ingweland.Fog.Dtos.Hoh.CityPlanner;
using Ingweland.Fog.Models.Hoh.Entities.City;
using Ingweland.Fog.Models.Hoh.Enums;
using Microsoft.Extensions.Logging;
using IMapper = AutoMapper.IMapper;

namespace Ingweland.Fog.Application.Core.Services.Hoh;

public class CityService(
    IHohCoreDataRepository hohCoreDataRepository,
    IBuildingTypeDtoFactory buildingTypeDtoFactory,
    IBuildingGroupDtoFactory buildingGroupDtoFactory,
    IWonderDtoFactory wonderDtoFactory,
    ICityPlannerDataFactory cityPlannerDataFactory,
    IMapper mapper,
    ILogger<CityService> logger) : ICityService
{
    public async Task<IReadOnlyCollection<BuildingTypeDto>> GetBuildingCategoriesAsync(CityId cityId)
    {
        var types = new List<BuildingType>
        {
            BuildingType.Home, BuildingType.Farm, BuildingType.Barracks, BuildingType.Workshop,
            BuildingType.CultureSite, BuildingType.Special, BuildingType.Beehive, BuildingType.Irrigation,
            BuildingType.Merchant, BuildingType.ExtractionPoint, BuildingType.FishingPier, BuildingType.GoldMine,
            BuildingType.PapyrusField, BuildingType.RiceFarm, BuildingType.Aviary, BuildingType.Quarry,
            BuildingType.RitualSite, BuildingType.CamelFarm, BuildingType.PresetIrrigation, BuildingType.Plantation,
            BuildingType.Pier,
        };
        var buildings = await hohCoreDataRepository.GetBuildingsAsync(cityId);
        var categories = buildings.Where(b => types.Contains(b.Type)).DistinctBy(b => b.Group).GroupBy(b => b.Type);
        return categories
            .Select(category => buildingTypeDtoFactory.Create(category.Key, cityId, category.ToList()))
            .ToList();
    }

    public async Task<BuildingGroupDto?> GetBuildingGroupAsync(CityId cityId, BuildingGroup group,
        CancellationToken cancellationToken = default)
    {
        var buildings = (await hohCoreDataRepository.GetGroupBuildingsAsync(cityId, group)).Where(b => b.Age.Index > 1)
            .OrderBy(b => b.Level).ToList();
        return buildings.Count == 0 ? null : buildingGroupDtoFactory.Create(group, buildings.First().Type, buildings);
    }

    public async Task<WonderDto?> GetWonderAsync(WonderId id)
    {
        var wonder = await hohCoreDataRepository.GetWonderAsync(id);
        if (wonder == null)
        {
            logger.LogError($"Failed to get wonder by id: {id}");
            return null;
        }

        return wonderDtoFactory.Create(wonder);
    }

    public Task<IReadOnlyCollection<Expansion>> GetExpansionsAsync(CityId cityId)
    {
        return hohCoreDataRepository.GetExpansions(cityId);
    }

    public async Task<CityPlannerDataDto?> GetCityPlannerDataAsync(CityId cityId)
    {
        var city = await hohCoreDataRepository.GetCity(cityId);
        if (city == null)
        {
            logger.LogError($"Failed to get city by id: {cityId}");
            return default;
        }

        var expansions = await hohCoreDataRepository.GetExpansions(cityId);
        var buildings = await GetBuildingsAsync(cityId);
        var customizations = await hohCoreDataRepository.GetBuildingCustomizations(cityId);
        var ages = await hohCoreDataRepository.GetAges();
        var wonders = await hohCoreDataRepository.GetWondersAsync();
        var expansionCosts = await hohCoreDataRepository.GetExpansionCosts(cityId);
        return cityPlannerDataFactory.Create(city, expansions, buildings, customizations, ages, wonders,
            expansionCosts);
    }

    public async Task<IReadOnlyCollection<BuildingDto>> GetBarracks(UnitType unitType)
    {
        var group = unitType.ToBuildingGroup();

        if (group == BuildingGroup.Undefined)
        {
            logger.LogError($"Failed to get building group for unit type: {unitType}");
            return [];
        }

        var barracks = await hohCoreDataRepository.GetGroupBuildingsAsync(CityId.Capital, group);
        return mapper.Map<List<BuildingDto>>(barracks);
    }

    public async Task<IReadOnlyCollection<BuildingDto>> GetBuildingsAsync(CityId cityId)
    {
        var buildings = await hohCoreDataRepository.GetBuildingsAsync(cityId);
        return mapper.Map<List<BuildingDto>>(buildings);
    }

    public async Task<IReadOnlyDictionary<CityId, IReadOnlyCollection<WonderBasicDto>>> GetWonderBasicDataAsync()
    {
        return new Dictionary<CityId, IReadOnlyCollection<WonderBasicDto>>
        {
            {
                CityId.China,
                new List<WonderBasicDto>
                {
                    mapper.Map<WonderBasicDto>(
                        await hohCoreDataRepository.GetWonderAsync(WonderId.China_ForbiddenCity)),
                    mapper.Map<WonderBasicDto>(await hohCoreDataRepository.GetWonderAsync(WonderId.China_GreatWall)),
                    mapper.Map<WonderBasicDto>(
                        await hohCoreDataRepository.GetWonderAsync(WonderId.China_TerracottaArmy)),
                }
            },
            {
                CityId.Egypt,
                new List<WonderBasicDto>
                {
                    mapper.Map<WonderBasicDto>(await hohCoreDataRepository.GetWonderAsync(WonderId.Egypt_AbuSimbel)),
                    mapper.Map<WonderBasicDto>(
                        await hohCoreDataRepository.GetWonderAsync(WonderId.Egypt_CheopsPyramid)),
                    mapper.Map<WonderBasicDto>(await hohCoreDataRepository.GetWonderAsync(WonderId.Egypt_GreatSphinx)),
                }
            },
            {
                CityId.Vikings,
                new List<WonderBasicDto>
                {
                    mapper.Map<WonderBasicDto>(
                        await hohCoreDataRepository.GetWonderAsync(WonderId.Vikings_DragonshipEllida)),
                    mapper.Map<WonderBasicDto>(await hohCoreDataRepository.GetWonderAsync(WonderId.Vikings_Valhalla)),
                    mapper.Map<WonderBasicDto>(await hohCoreDataRepository.GetWonderAsync(WonderId.Vikings_Yggdrasil)),
                }
            },
            {
                CityId.Mayas_Tikal,
                new List<WonderBasicDto>
                {
                    mapper.Map<WonderBasicDto>(await hohCoreDataRepository.GetWonderAsync(WonderId.Mayas_ChichenItza)),
                    mapper.Map<WonderBasicDto>(await hohCoreDataRepository.GetWonderAsync(WonderId.Mayas_SayilPalace)),
                    mapper.Map<WonderBasicDto>(await hohCoreDataRepository.GetWonderAsync(WonderId.Mayas_Tikal)),
                }
            },
            {
                CityId.Arabia_Petra,
                new List<WonderBasicDto>
                {
                    mapper.Map<WonderBasicDto>(await hohCoreDataRepository.GetWonderAsync(WonderId.Arabia_CityOfBrass)),
                    mapper.Map<WonderBasicDto>(
                        await hohCoreDataRepository.GetWonderAsync(WonderId.Arabia_NoriasOfHama)),
                    mapper.Map<WonderBasicDto>(await hohCoreDataRepository.GetWonderAsync(WonderId.Arabia_Petra)),
                }
            },
            {
                CityId.AncientEgyptEvent,
                new List<WonderBasicDto>
                {
                    mapper.Map<WonderBasicDto>(
                        await hohCoreDataRepository.GetWonderAsync(WonderId.AncientEgyptEvent_AnubisTemple)),
                }
            },
            {
                CityId.Ithaka,
                new List<WonderBasicDto>
                {
                    mapper.Map<WonderBasicDto>(
                        await hohCoreDataRepository.GetWonderAsync(WonderId.Ithaka_PenelopesHearth)),
                }
            },
        };
    }

    public async Task<IReadOnlyCollection<BuildingDto>> GetAllBarracks()
    {
        var unitTypes = new List<UnitType>
            {UnitType.Cavalry, UnitType.HeavyInfantry, UnitType.Infantry, UnitType.Ranged, UnitType.Siege};

        var barracks = new List<BuildingDto>();
        foreach (var unitType in unitTypes)
        {
            barracks.AddRange(await GetBarracks(unitType));
        }

        return barracks;
    }

    public Task<IReadOnlyCollection<CityDto>> GetCitiesAsync()
    {
        var cityIds = new List<CityId>
        {
            CityId.Capital, CityId.China, CityId.Egypt, CityId.Vikings, CityId.Mayas_Tikal.ToDefaultTechnologyCity(),
            CityId.Arabia_Petra.ToDefaultTechnologyCity(), CityId.AncientEgyptEvent, CityId.Ithaka,
        };
        return Task.FromResult(mapper.Map<IReadOnlyCollection<CityDto>>(cityIds));
    }
}
