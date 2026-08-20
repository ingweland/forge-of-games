using AutoMapper;
using Ingweland.Fog.Application.Client.Web.CityPlanner.Abstractions;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Client.Web.ViewModels;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh;
using Ingweland.Fog.Application.Core.CityPlanner.Abstractions;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Dtos.Hoh.PlayerCity;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Fog.Enums;
using Ingweland.Fog.Models.Hoh.Constants;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.CityPlanner.Inspirations;

public class CityInspirationsUiService(
    ICommonUiService commonUiService,
    IPersistenceService persistenceService,
    ICityPlannerService cityPlannerService,
    IPlayerCitySnapshotViewModelFactory playerCitySnapshotViewModelFactory,
    ICityPlannerDataService cityPlannerDataService,
    IMapper mapper) : ICityInspirationsUiService
{
    private static readonly IReadOnlyCollection<CitySnapshotSearchPreference> SearchPreferences =
        [CitySnapshotSearchPreference.Food, CitySnapshotSearchPreference.Coins, CitySnapshotSearchPreference.Goods];

    private static readonly List<CityProductionMetric> CityProductionMetrics =
    [
        CityProductionMetric.Storage, CityProductionMetric.OneHour, CityProductionMetric.OneDay,
        CityProductionMetric.StoragePerCityArea, CityProductionMetric.OneHourPerCityArea,
        CityProductionMetric.OneDayPerCityArea,
    ];

    public async Task<CityInspirationsSearchFormViewModel> GetSearchFormDataAsync()
    {
        var cities = await persistenceService.GetCities();
        var ages = await commonUiService.GetAgesAsync();
        var comingSoonAgeIndex = 16;
        if (ages.TryGetValue(AgeIds.COMING_SOON, out var comingSoonAge))
        {
            comingSoonAgeIndex = comingSoonAge.Index;
        }

        return new CityInspirationsSearchFormViewModel
        {
            Ages = ages.Values.Where(x => x.Index > 2 && x.Index < comingSoonAgeIndex).ToList(),
            Cities = cities,
            SearchPreferences =
                mapper.Map<IReadOnlyCollection<CitySnapshotSearchPreferenceViewModel>>(SearchPreferences),
            ProductionMetrics =
                mapper.Map<IReadOnlyCollection<LabeledValue<CityProductionMetric>>>(CityProductionMetrics),
        };
    }

    public async Task<IReadOnlyList<PlayerCitySnapshotBasicViewModel>> GetInspirationsAsync(
        CityInspirationsSearchRequest request, CancellationToken ct = default)
    {
        var ages = await commonUiService.GetAgesAsync();
        var snapshots = await cityPlannerService.GetInspirationsAsync(request, ct);
        return snapshots.Select(x =>
            playerCitySnapshotViewModelFactory.Create(x, ages.GetValueOrDefault(x.AgeId, AgeViewModel.Blank))).ToList();
    }

    public Task<HohCity?> GetPlayerCitySnapshotAsync(int snapshotId, CancellationToken ct = default)
    {
        return cityPlannerService.GetPlayerCitySnapshotAsync(snapshotId, ct);
    }

    public async Task<int> CalculateTotalAreaAsync(CityId cityId, int expansionCount)
    {
        var cityPlannerData = await cityPlannerDataService.GetCityPlannerDataAsync(cityId);
        var expansionSize = cityPlannerData.City.InitConfigs.Grid.ExpansionSize *
            cityPlannerData.City.InitConfigs.Grid.ExpansionSize;
        return expansionSize * expansionCount;
    }
}
