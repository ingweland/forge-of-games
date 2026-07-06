using System.Globalization;
using Ingweland.Fog.Application.Client.Web.CityPlanner.Abstractions;
using Ingweland.Fog.Application.Client.Web.Providers.Interfaces;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh;
using Ingweland.Fog.Application.Core.CityPlanner.Stats;
using Ingweland.Fog.Dtos.Hoh.City;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.CityPlanner.Stats;

public class AreaStatsViewModelFactory(
    IBuildingTypeIconUrlProvider buildingTypeIconUrlProvider,
    IAssetUrlProvider assetUrlProvider)
    : IAreaStatsViewModelFactory
{
    public AreaStatsViewModel Create(CityStats stats, IEnumerable<BuildingDto> buildings)
    {
        var buildingDic = buildings.GroupBy(b => b.Group).ToDictionary(g => g.Key, g => g.First());
        var areasByGroup = stats.AreasByGroup.Select(kvp =>
        {
            var building = buildingDic[kvp.Key];
            return (building.GroupName, kvp.Value.Count.ToString("N0"), kvp.Value.Area.ToString("N0"));
        }).OrderBy(t => t.GroupName).ToList();

        var areasByType = new List<IconLabelItemViewModel>();
        var sumOfSpecial = 0;
        foreach (var kvp in stats.AreasByType)
        {
            if (kvp.Key is BuildingType.Home or BuildingType.Farm or BuildingType.Barracks or BuildingType.Workshop
                or BuildingType.CultureSite or BuildingType.Plantation)
            {
                areasByType.Add(new IconLabelItemViewModel
                {
                    Label = kvp.Value.ToString("N0"),
                    IconUrl = buildingTypeIconUrlProvider.GetIcon(kvp.Key),
                });
            }
            else
            {
                sumOfSpecial += kvp.Value;
            }
        }

        if (sumOfSpecial > 0)
        {
            areasByType.Add(new IconLabelItemViewModel
            {
                Label = sumOfSpecial.ToString("N0"),
                IconUrl = buildingTypeIconUrlProvider.GetIcon(BuildingType.Special),
            });
        }

        IconLabelItemViewModel? premiumExpansions = null;
        IconLabelItemViewModel? premiumExpansionCost = null;
        if (stats.PremiumExpansionCount > 0)
        {
            premiumExpansions = new IconLabelItemViewModel
            {
                Label = stats.PremiumExpansionCount.ToString("N0"),
                IconUrl = assetUrlProvider.GetHohIconUrl("icon_flat_expansion"),
            };
            premiumExpansionCost = new IconLabelItemViewModel
            {
                Label = stats.PremiumExpansionCost.ToString("N0"),
                IconUrl = assetUrlProvider.GetHohIconUrl("icon_premium"),
            };
        }

        return new AreaStatsViewModel
        {
            AreasByType = areasByType,
            AreasByGroup = areasByGroup,
            TotalArea = stats.TotalArea.ToString(CultureInfo.InvariantCulture),
            PremiumExpansionCount = premiumExpansions,
            PremiumExpansionCost = premiumExpansionCost,
        };
    }
}
