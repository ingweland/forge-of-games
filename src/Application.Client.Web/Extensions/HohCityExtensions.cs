using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.Extensions;

public static class HohCityExtensions
{
    public static string GetBuildingTypeIconId(this BuildingType buildingType)
    {
        return buildingType switch
        {
            BuildingType.Aviary => "icon_flat_aviary",
            BuildingType.Barracks => "icon_flat_barracks",
            BuildingType.Beehive => "icon_flat_beehive",
            BuildingType.CamelFarm => "icon_flat_camelFarm",
            BuildingType.CultureSite => "icon_flat_cultureSite",
            BuildingType.Farm => "icon_flat_farm",
            BuildingType.FishingPier => "icon_flat_fishingPier",
            BuildingType.GoldMine => "icon_flat_goldMine",
            BuildingType.Home => "icon_flat_home",
            BuildingType.Irrigation => "icon_flat_irrigation",
            BuildingType.Merchant => "icon_flat_merchant",
            BuildingType.PapyrusField => "icon_flat_papyrusField",
            BuildingType.PresetIrrigation => "icon_flat_irrigation",
            BuildingType.Quarry => "icon_flat_quarry",
            BuildingType.RiceFarm => "icon_flat_riceFarm",
            BuildingType.Special => "icon_flat_special",
            BuildingType.Workshop => "icon_flat_workshop",
            BuildingType.Plantation => "icon_flat_plantation",
            BuildingType.Pier => "icon_flat_pier",
            _ => string.Empty,
        };
    }
}
