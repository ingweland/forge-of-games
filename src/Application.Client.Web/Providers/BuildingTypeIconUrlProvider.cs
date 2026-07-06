using Ingweland.Fog.Application.Client.Web.Providers.Interfaces;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Client.Web.Providers;

public class BuildingTypeIconUrlProvider(IAssetUrlProvider assetUrlProvider) : IBuildingTypeIconUrlProvider
{
    public string GetIcon(BuildingType buildingType)
    {
        var id = buildingType switch
        {
            BuildingType.Barracks => "icon_flat_barracks",
            BuildingType.Beehive => "",
            BuildingType.CamelFarm => "icon_flat_camelFarm",
            BuildingType.CityHall => "icon_flat_special",
            BuildingType.Collectable => "icon_flat_special",
            BuildingType.CultureSite => "icon_flat_cultureSite",
            BuildingType.Evolving => "icon_flat_special",
            BuildingType.ExtractionPoint => "",
            BuildingType.Farm => "icon_flat_farm",
            BuildingType.FishingPier => "icon_flat_fishingPier",
            BuildingType.GoldMine => "",
            BuildingType.Home => "icon_flat_home",
            BuildingType.Irrigation => "",
            BuildingType.Merchant => "icon_flat_merchant",
            BuildingType.PapyrusField => "",
            BuildingType.PresetIrrigation => "",
            BuildingType.RiceFarm => "",
            BuildingType.Runestone => "",
            BuildingType.Special => "icon_flat_special",
            BuildingType.Workshop => "icon_flat_workshop",
            BuildingType.Plantation => "icon_flat_plantation",
            BuildingType.Pier => "icon_flat_pier",
            _ => string.Empty,
        };

        return assetUrlProvider.GetHohIconUrl(id);
    }
}
