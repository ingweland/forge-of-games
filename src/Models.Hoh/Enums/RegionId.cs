// ReSharper disable InconsistentNaming

using ProtoBuf;

namespace Ingweland.Fog.Models.Hoh.Enums;

[ProtoContract]
public enum RegionId
{
    Undefined = 0,
    DesertDelta_1,
    DesertDelta_2,
    DesertDelta_3,
    DesertDelta_4,
    DesertDelta_5,
    DesertDelta_6,
    DesertDelta_7,
    DesertDelta_8,
    EasternValley_1,
    EasternValley_2,
    EasternValley_3,
    EasternValley_4,
    EasternValley_5,
    EasternValley_6,
    EasternValley_7,
    EasternValley_8,
    EmeraldHighlands_1,
    EmeraldHighlands_2,
    EmeraldHighlands_3,
    EmeraldHighlands_4,
    EmeraldHighlands_5,
    EmeraldHighlands_6,
    EmeraldHighlands_7,
    EmeraldHighlands_8,
    FrozenFjord_1,
    FrozenFjord_2,
    FrozenFjord_3,
    FrozenFjord_4,
    FrozenFjord_5,
    FrozenFjord_6,
    FrozenFjord_7,
    FrozenFjord_8,
    Panganea_1,
    Panganea_2,
    Panganea_3,
    Panganea_4,
    Panganea_5,
    Panganea_6,
    Panganea_7,
    Panganea_8,
    TeslaStormBlue,
    TeslaStormGreen,
    TeslaStormPurple,
    TeslaStormRed,
    TeslaStormYellow,
    VolcanicJungle_1,
    VolcanicJungle_2,
    VolcanicJungle_3,
    VolcanicJungle_4,
    VolcanicJungle_5,
    VolcanicJungle_6,
    VolcanicJungle_7,
    VolcanicJungle_8,
    SiegeOfOrleans,
    SpartasLastStand,
    FallOfTroy,
    AncientEgyptDungeon,
    ScyllaDungeon,
}

public enum CampaignRegionGroup
{
    Panganea,
    DesertDelta,
    EasternValley,
    VolcanicJungle,
    FrozenFjord,
    EmeraldHighlands,
}

public static class CampaignRegionGroups
{
    public static IReadOnlyDictionary<CampaignRegionGroup, IReadOnlyCollection<RegionId>> All { get; } =
        new Dictionary<CampaignRegionGroup, IReadOnlyCollection<RegionId>>
        {
            [CampaignRegionGroup.DesertDelta] = new List<RegionId>
            {
                RegionId.DesertDelta_1, RegionId.DesertDelta_2, RegionId.DesertDelta_3, RegionId.DesertDelta_4,
                RegionId.DesertDelta_5, RegionId.DesertDelta_6, RegionId.DesertDelta_7, RegionId.DesertDelta_8,
            },
            [CampaignRegionGroup.EasternValley] = new List<RegionId>
            {
                RegionId.EasternValley_1, RegionId.EasternValley_2, RegionId.EasternValley_3, RegionId.EasternValley_4,
                RegionId.EasternValley_5, RegionId.EasternValley_6, RegionId.EasternValley_7, RegionId.EasternValley_8,
            },
            [CampaignRegionGroup.EmeraldHighlands] = new List<RegionId>
            {
                RegionId.EmeraldHighlands_1, RegionId.EmeraldHighlands_2, RegionId.EmeraldHighlands_3,
                RegionId.EmeraldHighlands_4, RegionId.EmeraldHighlands_5, RegionId.EmeraldHighlands_6,
                RegionId.EmeraldHighlands_7, RegionId.EmeraldHighlands_8,
            },
            [CampaignRegionGroup.FrozenFjord] = new List<RegionId>
            {
                RegionId.FrozenFjord_1, RegionId.FrozenFjord_2, RegionId.FrozenFjord_3, RegionId.FrozenFjord_4,
                RegionId.FrozenFjord_5, RegionId.FrozenFjord_6, RegionId.FrozenFjord_7, RegionId.FrozenFjord_8,
            },
            [CampaignRegionGroup.Panganea] = new List<RegionId>
            {
                RegionId.Panganea_1, RegionId.Panganea_2, RegionId.Panganea_3, RegionId.Panganea_4,
                RegionId.Panganea_5, RegionId.Panganea_6, RegionId.Panganea_7, RegionId.Panganea_8,
            },
            [CampaignRegionGroup.VolcanicJungle] = new List<RegionId>
            {
                RegionId.VolcanicJungle_1, RegionId.VolcanicJungle_2, RegionId.VolcanicJungle_3,
                RegionId.VolcanicJungle_4, RegionId.VolcanicJungle_5, RegionId.VolcanicJungle_6,
                RegionId.VolcanicJungle_7, RegionId.VolcanicJungle_8,
            },
        };
}
