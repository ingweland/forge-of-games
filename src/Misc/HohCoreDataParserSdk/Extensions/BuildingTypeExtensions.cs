using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.HohCoreDataParserSdk.Extensions;

public static class BuildingTypeExtensions
{
    public static BuildingType ToBuildingType(this string value)
    {
        return value switch
        {
            "barracks" => BuildingType.Barracks,
            "camelFarm" => BuildingType.CamelFarm,
            "cityHall" => BuildingType.CityHall,
            "collectable" => BuildingType.Collectable,
            "cultureSite" => BuildingType.CultureSite,
            "evolving" => BuildingType.Evolving,
            "extractionPoint" => BuildingType.ExtractionPoint,
            "farm" => BuildingType.Farm,
            "goldMine" => BuildingType.GoldMine,
            "home" => BuildingType.Home,
            "irrigation" => BuildingType.Irrigation,
            "merchant" => BuildingType.Merchant,
            "papyrusField" => BuildingType.PapyrusField,
            "presetIrrigation" => BuildingType.PresetIrrigation,
            "riceFarm" => BuildingType.RiceFarm,
            "special" => BuildingType.Special,
            "workshop" => BuildingType.Workshop,
            "runestone" => BuildingType.Runestone,
            "beehive" => BuildingType.Beehive,
            "fishingPier" => BuildingType.FishingPier,
            "quarry" => BuildingType.Quarry,
            "aviary" => BuildingType.Aviary,
            "ritualSite" => BuildingType.RitualSite,
            "plantation" => BuildingType.Plantation,
            "pier" => BuildingType.Pier,
            _ => throw new Exception($"Cannot map building type: {value}"),
        };
    }
}
