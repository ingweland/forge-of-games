using ProtoBuf;

namespace Ingweland.Fog.Models.Hoh.Enums;

[ProtoContract]
public enum BuildingType
{
    Undefined = 0,
    Aviary,
    Barracks,
    Beehive,
    CamelFarm,
    CityHall,
    Collectable,
    CultureSite,
    Evolving,
    ExtractionPoint,
    Farm,
    FishingPier,
    GoldMine,
    Home,
    Irrigation,
    Merchant,
    PapyrusField,
    PresetIrrigation,
    Quarry,
    RiceFarm,
    RitualSite,
    Runestone,
    Special,
    Workshop,
    Plantation,
    Pier,
}
