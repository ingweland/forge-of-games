using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Core.Extensions;

public static class BuildingTypeExtensions
{
    public static string ToStringRepresentation(this BuildingType buildingType)
    {
        return buildingType switch
        {
            BuildingType.Barracks => "barracks",
            BuildingType.CamelFarm => "camelFarm",
            BuildingType.CityHall => "cityHall",
            BuildingType.Collectable => "collectable",
            BuildingType.CultureSite => "cultureSite",
            BuildingType.Evolving => "evolving",
            BuildingType.ExtractionPoint => "extractionPoint",
            BuildingType.Farm => "farm",
            BuildingType.GoldMine => "goldMine",
            BuildingType.Home => "home",
            BuildingType.Irrigation => "irrigation",
            BuildingType.Merchant => "merchant",
            BuildingType.PapyrusField => "papyrusField",
            BuildingType.PresetIrrigation => "presetIrrigation",
            BuildingType.RiceFarm => "riceFarm",
            BuildingType.Special => "special",
            BuildingType.Workshop => "workshop",
            BuildingType.Runestone => "runestone",
            BuildingType.Beehive => "beehive",
            BuildingType.FishingPier => "fishingPier",
            BuildingType.Aviary => "aviary",
            BuildingType.Quarry => "quarry",
            BuildingType.RitualSite => "ritualSite",
            BuildingType.Plantation => "plantation",
            BuildingType.Pier => "pier",
            _ => string.Empty,
        };
    }
}
