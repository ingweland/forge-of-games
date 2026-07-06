using AutoMapper;
using Ingweland.Fog.Application.Client.Web.Factories.Interfaces;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh.City;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.Services.Hoh;

public class CityUiService(
    ICityService cityService,
    IWonderViewModelViewModelFactory wonderViewModelViewModelFactory,
    IMapper mapper) : ICityUiService
{
    private readonly Dictionary<string, BuildingGroupViewModel> _buildingGroupViewModelCache = new();

    public async Task<IReadOnlyCollection<BuildingTypeViewModel>> GetBuildingCategoriesAsync(CityId cityId)
    {
        var types = await cityService.GetBuildingCategoriesAsync(cityId);
        return mapper.Map<IReadOnlyCollection<BuildingTypeViewModel>>(types.OrderBy(t => t.Name));
    }

    public async Task<IReadOnlyCollection<CityBuildingGroupsViewModel>> GetCityBuildingGroupsAsync()
    {
        var cityIds = new List<CityId>
        {
            CityId.Capital, CityId.China, CityId.Egypt, CityId.Vikings, CityId.Mayas_Tikal, CityId.Arabia_Petra,
            CityId.AncientEgyptEvent, CityId.Ithaka,
        };
        var result = new List<CityBuildingGroupsViewModel>();
        foreach (var cityId in cityIds)
        {
            result.Add(await GetCityBuildingGroupsAsync(cityId));
        }

        return result;
    }

    public async Task<IReadOnlyCollection<WonderGroupViewModel>> GetWonderGroupsAsync()
    {
        var wonders = await cityService.GetWonderBasicDataAsync();
        return wonders
            .Select(kvp => new WonderGroupViewModel
            {
                CityId = kvp.Key, CityName = kvp.Value.First().CityName,
                Wonders = mapper.Map<IReadOnlyCollection<WonderBasicViewModel>>(
                    kvp.Value.OrderBy(wbd => wbd.WonderName)),
            })
            .ToList();
    }

    public async Task<BuildingGroupViewModel?> GetBuildingGroupAsync(CityId cityId, BuildingGroup group,
        CancellationToken cancellationToken = default)
    {
        var key = $"{cityId}_{group}";
        if (_buildingGroupViewModelCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var buildingGroup = await cityService.GetBuildingGroupAsync(cityId, group, cancellationToken);
        if (buildingGroup == null)
        {
            return null;
        }

        var vm = mapper.Map<BuildingGroupViewModel>(buildingGroup);
        _buildingGroupViewModelCache.Add(key, vm);
        return vm;
    }

    public async Task<WonderViewModel?> GetWonderAsync(WonderId id)
    {
        var wonder = await cityService.GetWonderAsync(id);
        if (wonder == null)
        {
            return null;
        }

        return wonderViewModelViewModelFactory.Create(wonder);
    }

    public async Task<CityBuildingGroupsViewModel> GetCityBuildingGroupsAsync(CityId cityId)
    {
        var types = await cityService.GetBuildingCategoriesAsync(cityId);
        var typeViewModels = mapper.Map<IReadOnlyCollection<BuildingTypeViewModel>>(types.OrderBy(t => t.Name));
        return new CityBuildingGroupsViewModel
        {
            CityId = cityId,
            CityName = types.First().CityName,
            BuildingTypes = typeViewModels,
        };
    }
}
