using System.Globalization;
using Ingweland.Fog.Application.Core.Enums.Hoh;
using Ingweland.Fog.Application.Core.Extensions;
using Ingweland.Fog.Application.Core.Helpers;
using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Core.Services;

public class HohGameLocalizationService(IHohGameLocalizationDataRepository localizationDataRepository)
    : IHohGameLocalizationService
{
    public string GetContinentName(ContinentId id)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.Continents, HohLocalizationProperty.Name,
            $"continent.{id}");
        return GetValue(key) ?? id.ToString();
    }

    public string GetDifficultyName(Difficulty difficulty)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.Difficulties, HohLocalizationProperty.Name,
            $"difficulty.{difficulty}");
        return GetValue(key) ?? difficulty.ToString();
    }

    public string GetTreasureHuntDifficulty(int difficulty)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.TreasureHuntDifficulties,
            HohLocalizationProperty.Name,
            $"Difficulty_{difficulty}");
        return GetValue(key) ?? difficulty.ToString();
    }

    public string GetTreasureHuntStageName(int stage)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.TreasureHuntDifficultiesPanel, "Stage");
        var value = GetValue(key);
        return value != null ? $"{value} {stage + 1}" : (stage + 1).ToString();
    }

    public string GetCityName(CityId id)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.Cities, HohLocalizationProperty.Name,
            $"City_{id}");
        return GetValue(key) ?? id.ToString();
    }

    public string GetWonderName(string id)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.Wonders, HohLocalizationProperty.Name,
            $"Wonder_{id}");
        return GetValue(key) ?? id;
    }

    public string GetEquipmentSlotTypeName(EquipmentSlotType slotType)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.EquipmentSlotNames, slotType.ToString());
        return GetValue(key) ?? slotType.ToString();
    }

    public string GetStatAttributeAbbreviation(StatAttribute statAttribute)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.UnitStats, HohLocalizationProperty.Abbrev,
            $"unit_stat.{statAttribute}");
        return GetValue(key) ?? statAttribute.ToString();
    }

    public string GetStatAttributeName(StatAttribute statAttribute)
    {
        var suffixLenght = "Bonus".Length;
        var s = statAttribute switch
        {
            StatAttribute.AttackBonus or StatAttribute.BaseDamageBonus or StatAttribute.DefenseBonus
                or StatAttribute.MaxHitPointsBonus => $"{statAttribute.ToString()[..^suffixLenght]}_Percent",
            _ => statAttribute.ToString(),
        };
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.UnitStats, $"unit_stat.{s}");
        return GetValue(key) ?? s;
    }

    public string GetUnitStatName(UnitStatType unitStat)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.UnitStats, $"unit_stat.{unitStat}");
        return GetValue(key) ?? unitStat.ToString();
    }

    public string GetEquipmentSetName(EquipmentSet set)
    {
        var s = set.ToString();
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.EquipmentSets,
            HohLocalizationProperty.Name,
            $"equipment_set.{s}");
        return GetValue(key) ?? s;
    }

    public string GetConcreteEquipmentSetName(EquipmentSet set, EquipmentSlotType slot)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.EquipmentSets,
            HohLocalizationProperty.Name,
            $"equipment_set.{set}_{slot}");
        return GetValue(key) ?? $"{set} {slot}";
    }

    public string GetPvpTierName(PvpTier tier)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.PvpTiers, HohLocalizationProperty.Name,
            tier.ToString());
        return GetValue(key) ?? tier.ToString();
    }

    public string GetTreasureHuntLeagueName(TreasureHuntLeague league)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.TreasureHuntRankingPanel,
            league.ToString());
        return GetValue(key) ?? league.ToString();
    }

    public string GetWoaTierName(WoaTier tier)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.WoATier, tier.ToString());
        return GetValue(key) ?? tier.ToString();
    }

    public string GetHeroName(string id)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.Heroes, HohLocalizationProperty.Name,
            $"hero.{id}");
        return GetValue(key) ?? id;
    }

    public string GetRegionName(RegionId id)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.Regions, HohLocalizationProperty.Name,
            $"region.{id}");
        return GetValue(key) ?? id.ToString();
    }

    public string GetBattleEventName(string eventId)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.IngameEvents, HohLocalizationProperty.Name,
            eventId);
        return GetValue(key) ?? eventId;
    }

    public string GetResourceName(string resourceId)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.Resources, HohLocalizationProperty.Name,
            resourceId);
        return GetValue(key) ?? resourceId;
    }

    public string GetUnitName(string name)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.Units, HohLocalizationProperty.Name, name);
        return GetValue(key) ?? name;
    }

    public string GetRelicName(string relicId)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.Relics, HohLocalizationProperty.Name,
            relicId);
        return GetValue(key) ?? relicId;
    }

    public string GetHeroClassName(string name)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.HeroClass, name);
        return GetValue(key) ?? name;
    }

    public string GetBattleAbilityDescription(string abilityDescriptionId)
    {
        var keyName = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.AbilityDescriptions,
            HohLocalizationProperty.Name, abilityDescriptionId);
        var keyDesc = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.AbilityDescriptions,
            HohLocalizationProperty.Desc, abilityDescriptionId);
        return GetValue(keyName) ?? GetValue(keyDesc) ?? abilityDescriptionId;
    }

    public string GetBattleAbilityName(string abilityId)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.Abilities,
            HohLocalizationProperty.Name, abilityId);
        return GetValue(key) ?? abilityId;
    }

    public string GetUnitType(UnitType unitType)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.UnitTypes, HohLocalizationProperty.Name,
            unitType.ToString());
        return GetValue(key) ?? unitType.ToString();
    }

    public string GetBuildingName(string name)
    {
        // school localization has only one value for some reason
        // which does not confirm to the rest of the buildings
        if (name.StartsWith("Building_BronzeAge_Collectable_SchoolV2"))
        {
            name = "Building_BronzeAge_Collectable_SchoolV2_1";
        }

        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.Buildings, HohLocalizationProperty.Name,
            name);
        return GetValue(key) ?? name;
    }

    public string GetAgeName(string ageId)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.Ages, HohLocalizationProperty.Name,
            ageId);
        return GetValue(key) ?? ageId;
    }

    public string GetBuildingType(BuildingType buildingType, bool plural = false)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.BuildingTypes,
            HohLocalizationProperty.Name,
            buildingType.ToStringRepresentation());
        return GetValue(key, plural) ?? buildingType.ToString();
    }

    public string GetBuildingGroup(BuildingGroup buildingGroup, bool plural = false)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.BuildingGroups,
            HohLocalizationProperty.Name,
            buildingGroup.ToStringRepresentation());
        return GetValue(key, plural) ?? buildingGroup.ToString();
    }

    public string GetTechnologyName(string technologyId)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.Technologies, HohLocalizationProperty.Name,
            technologyId);
        return GetValue(key) ?? technologyId;
    }

    public string GetBuildingCustomizationName(string customizationId)
    {
        var key = HohLocalizationKeyBuilder.BuildKey(HohLocalizationCategory.BuildingCustomizations,
            HohLocalizationProperty.Name,
            customizationId);
        return GetValue(key) ?? customizationId;
    }

    private string? GetValue(string key, bool plural = false)
    {
        if (!localizationDataRepository.GetEntries(CultureInfo.CurrentCulture.Name).TryGetValue(key, out var values))
        {
            return null;
        }

        if (!plural)
        {
            return values.First();
        }

        return values.Count > 1 ? values.Skip(1).First() : values.First();
    }
}
