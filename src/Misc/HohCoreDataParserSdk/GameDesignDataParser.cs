using AutoMapper;
using Ingweland.Fog.HohCoreDataParserSdk.Converters;
using Ingweland.Fog.Inn.Models.Hoh;
using Ingweland.Fog.Models.Hoh.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Battle;
using Ingweland.Fog.Models.Hoh.Entities.City;
using Ingweland.Fog.Models.Hoh.Entities.Equipment;
using Ingweland.Fog.Models.Hoh.Entities.Relics;
using Ingweland.Fog.Models.Hoh.Entities.Research;
using Ingweland.Fog.Models.Hoh.Entities.Units;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.Shared.Helpers.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.HohCoreDataParserSdk;

public class GameDesignDataParser(
    IProtobufSerializer protobufSerializer,
    IMapper mapper,
    ILogger<GameDesignDataParser> logger)
{
    private static readonly HashSet<string> HeroesToSkip =
    [
    ];

    private static readonly Dictionary<string, string> CurrentToLegacyHeroUnitIdMap = new()
    {
        ["unit.Unit_WilliamTellLegendary"] = "unit.Unit_WilliamTell_5",
    };

    public byte[] Parse(byte[] gameDesignData, IReadOnlyCollection<byte[]> startupData)
    {
        if (HeroesToSkip.Count > 0)
        {
            logger.LogInformation("");
            logger.LogInformation("===================================");
            logger.LogInformation("");
            logger.LogInformation("Skipping following Heroes: \r\n{heroes}", string.Join(',', HeroesToSkip));
            logger.LogInformation("");
            logger.LogInformation("===================================");
            logger.LogInformation("");
        }

        logger.LogInformation("Starting parsing game design data.");

        var communicationDto = CommunicationDto.Parser.ParseFrom(gameDesignData);
        var gdr = communicationDto.GameDesignResponse;

        var startups = startupData.Select(x => CommunicationDto.Parser.ParseFrom(x)).ToList();

        var data = Parse(gdr, startups);
        var result = protobufSerializer.SerializeToBytes(data);

        logger.LogInformation("Completed parsing game design data.");

        return result;
    }

    private static IList<BuildingCustomization> BuildingCustomizations(IMapper mapper, GameDesignResponse gdr,
        IDictionary<string, Age> ages,
        IDictionary<string, Unit> units)
    {
        return mapper.Map<IList<BuildingCustomization>>(gdr.BuildingCustomizationDefinitions, opt =>
        {
            opt.Items.Add(ContextKeys.AGES, ages);
            opt.Items.Add(ContextKeys.UNITS, units);
            opt.Items.Add(ContextKeys.HERO_BUILDING_BOOST_COMPONENTS,
                gdr.HeroBuildingBoostComponents.ToDictionary(hbbc => hbbc.Id));
            opt.Items.Add(ContextKeys.HERO_ABILITY_TRAINING_COMPONENTS,
                gdr.HeroAbilityTrainingComponents.ToDictionary(hatc => hatc.Id));
            opt.Items.Add(ContextKeys.DYNAMIC_FLOAT_VALUE_DEFINITIONS, gdr.DynamicFloatValueDefinitions);
        });
    }

    private static IList<Building> CreateBuildings(IMapper mapper, GameDesignResponse gdr,
        IDictionary<string, Age> ages,
        IDictionary<string, Unit> units)
    {
        var buildings = mapper.Map<IList<Building>>(gdr.BuildingDefinitions, opt =>
        {
            opt.Items.Add(ContextKeys.AGES, ages);
            opt.Items.Add(ContextKeys.UNITS, units);
            opt.Items.Add(ContextKeys.HERO_BUILDING_BOOST_COMPONENTS,
                gdr.HeroBuildingBoostComponents.ToDictionary(hbbc => hbbc.Id));
            opt.Items.Add(ContextKeys.HERO_ABILITY_TRAINING_COMPONENTS,
                gdr.HeroAbilityTrainingComponents.ToDictionary(hatc => hatc.Id));
            opt.Items.Add(ContextKeys.DYNAMIC_FLOAT_VALUE_DEFINITIONS, gdr.DynamicFloatValueDefinitions);
        });
        foreach (var building in buildings)
        {
            var upgradeComponent = building.Components.OfType<UpgradeComponent>()
                .FirstOrDefault(uc => uc.NextBuildingId != building.Id);
            if (upgradeComponent == null)
            {
                continue;
            }

            building.Components.Remove(upgradeComponent);
            var targetBuilding = buildings.FirstOrDefault(b => b.Id == upgradeComponent.NextBuildingId);
            if (targetBuilding != null)
            {
                targetBuilding.Components.Add(upgradeComponent);
            }
        }

        return buildings;
    }

    private static IList<ExpansionCosts> CreateExpansionCosts(IMapper mapper, GameDesignResponse gdr)
    {
        return mapper.Map<IList<ExpansionCosts>>(gdr.ExpansionCosts, opt =>
        {
            opt.Items.Add(ContextKeys.HERO_BUILDING_BOOST_COMPONENTS,
                gdr.HeroBuildingBoostComponents.ToDictionary(hbbc => hbbc.Id));
            opt.Items.Add(ContextKeys.HERO_ABILITY_TRAINING_COMPONENTS,
                gdr.HeroAbilityTrainingComponents.ToDictionary(hatc => hatc.Id));
            opt.Items.Add(ContextKeys.DYNAMIC_FLOAT_VALUE_DEFINITIONS, gdr.DynamicFloatValueDefinitions);
        });
    }

    private static IList<CityDefinition> CreateCities(IMapper mapper, GameDesignResponse gdr)
    {
        return mapper.Map<IList<CityDefinition>>(gdr.CityDefinitions, opt =>
        {
            opt.Items.Add(ContextKeys.HERO_BUILDING_BOOST_COMPONENTS,
                gdr.HeroBuildingBoostComponents.ToDictionary(hbbc => hbbc.Id));
            opt.Items.Add(ContextKeys.HERO_ABILITY_TRAINING_COMPONENTS,
                gdr.HeroAbilityTrainingComponents.ToDictionary(hatc => hatc.Id));
            opt.Items.Add(ContextKeys.DYNAMIC_FLOAT_VALUE_DEFINITIONS, gdr.DynamicFloatValueDefinitions);
        });
    }

    private static IList<Expansion> CreateExpansions(IMapper mapper, GameDesignResponse gdr)
    {
        return mapper.Map<IList<Expansion>>(gdr.ExpansionDefinitions);
    }

    private static IList<BattleAbility> CreateHeroAbilities(IMapper mapper, GameDesignResponse gdr)
    {
        return mapper.Map<IList<BattleAbility>>(gdr.BattleAbilityDefinitions);
    }

    private static IList<HeroBattleAbilityComponent> CreateHeroAbilityComponents(IMapper mapper,
        GameDesignResponse gdr)
    {
        return mapper.Map<IList<HeroBattleAbilityComponent>>(gdr.HeroBattleAbilityComponents);
    }

    private static IList<HeroAwakeningComponent> CreateHeroAwakeningComponents(IMapper mapper,
        GameDesignResponse gdr)
    {
        return mapper.Map<IList<HeroAwakeningComponent>>(gdr.HeroAwakeningComponents);
    }

    private static IList<Technology> CreateTechnologies(IMapper mapper, GameDesignResponse gdr,
        List<CommunicationDto> startups,
        IDictionary<string, Age> ages)
    {
        var startupTechnologies =
            startups.SelectMany(x =>
                    x.InGameEvents.SelectMany(y =>
                        y.EventDefinition.EventCityComponents.SelectMany(h => h.Technologies)))
                .DistinctBy(x => x.Id);
        var allTechs = gdr.TechnologyDefinitions.Concat(startupTechnologies);
        return mapper.Map<IList<Technology>>(allTechs,
            opt => { opt.Items.Add(ContextKeys.AGES, ages); });
    }

    private static List<TreasureHuntDifficultyData> CreateTreasureHuntBattles(IMapper mapper, GameDesignResponse gdr,
        IDictionary<string, Unit> units)
    {
        var treasureHuntBattleDefinitions = gdr.HeroBattleDefinitions
            .Where(hbd => hbd.Id.StartsWith("hero_battle.Encounter"))
            .OrderBy(hbd => hbd.Id);
        var battles = mapper.Map<IList<BattleDetails>>(treasureHuntBattleDefinitions, opt =>
        {
            opt.Items.Add(ContextKeys.BATTLE_WAVES_DEFINITIONS, gdr.HeroBattleWaveDefinitions);
            opt.Items.Add(ContextKeys.UNITS, units);
        });
        var difficulties = new List<TreasureHuntDifficultyData>();
        var difficultyLevels = battles
            .Select(bd =>
            {
                var parts = bd.Id.Split('_');
                return parts[2];
            })
            .ToHashSet();
        foreach (var difficultyLevel in difficultyLevels)
        {
            var stages = new List<TreasureHuntStage>();
            var difficultyBattles = battles
                .Where(bd =>
                {
                    var parts = bd.Id.Split('_');
                    if (difficultyLevel == "9" && parts[^1] != "New")
                    {
                        return false;
                    }

                    return parts[2] == difficultyLevel;
                }).ToList();
            for (var i = 0; i < 4; i++)
            {
                var stageBattles = difficultyBattles
                    .Where(bd =>
                    {
                        var parts = bd.Id.Split('_');
                        return parts[3] == i.ToString();
                    });
                stages.Add(new TreasureHuntStage
                {
                    Index = i,
                    Battles = stageBattles.OrderBy(bd =>
                    {
                        var parts = bd.Id.Split('_');
                        return int.Parse(parts[4]);
                    }).ToList(),
                });
            }

            difficulties.Add(new TreasureHuntDifficultyData
            {
                Difficulty = int.Parse(difficultyLevel),
                Stages = stages,
            });
        }

        return difficulties;
    }

    private static IList<Wonder> CreateWonders(IMapper mapper, GameDesignResponse gdr)
    {
        return mapper.Map<IList<Wonder>>(gdr.ReworkedWonderDefinitions, opt =>
        {
            opt.Items.Add(ContextKeys.HERO_BUILDING_BOOST_COMPONENTS,
                gdr.HeroBuildingBoostComponents.ToDictionary(hbbc => hbbc.Id));
            opt.Items.Add(ContextKeys.HERO_ABILITY_TRAINING_COMPONENTS,
                gdr.HeroAbilityTrainingComponents.ToDictionary(hatc => hatc.Id));
            opt.Items.Add(ContextKeys.DYNAMIC_FLOAT_VALUE_DEFINITIONS, gdr.DynamicFloatValueDefinitions);
        });
    }

    private static IList<World> CreateWorlds(IMapper mapper, GameDesignResponse gdr, IDictionary<string, Age> ages,
        IDictionary<string, Unit> units)
    {
        var encounters =
            mapper.Map<IList<Encounter>>(gdr.EncounterDefinitions, opt =>
            {
                opt.Items.Add(ContextKeys.BATTLES_DEFINITIONS, gdr.HeroBattleDefinitions);
                opt.Items.Add(ContextKeys.BATTLE_WAVES_DEFINITIONS, gdr.HeroBattleWaveDefinitions);
                opt.Items.Add(ContextKeys.UNITS, units);
            });
        var regions = mapper.Map<IList<Region>>(gdr.RegionDefinitions, opt =>
        {
            opt.Items.Add(ContextKeys.AGES, ages);
            opt.Items.Add(ContextKeys.CONTINENT_DEFINITIONS, gdr.ContinentDefinitions);
            opt.Items.Add(ContextKeys.ENCOUNTERS, encounters);
        });
        var continents = mapper.Map<IList<Continent>>(gdr.ContinentDefinitions, opt =>
        {
            opt.Items.Add(ContextKeys.WORLD_DEFINITIONS, gdr.WorldDefinitions);
            opt.Items.Add(ContextKeys.REGIONS, regions);
        });

        return mapper.Map<IList<World>>(gdr.WorldDefinitions,
            opt => { opt.Items.Add(ContextKeys.CONTINENTS, continents); });
    }

    private Data Parse(GameDesignResponse gdr, List<CommunicationDto> startups)
    {
        var ages = mapper.Map<IList<Age>>(gdr.AgeDefinitions).ToDictionary(a => a.Id);
        var resources = mapper
            .Map<IList<Resource>>(gdr.ResourceDefinitions, opt => opt.Items.Add(ContextKeys.AGES, ages));
        var units = mapper.Map<IList<Unit>>(gdr.HeroUnitDefinitions).ToDictionary(r => r.Id);
        var worlds = CreateWorlds(mapper, gdr, ages, units);
        var technologies = CreateTechnologies(mapper, gdr, startups, ages);
        var buildings = CreateBuildings(mapper, gdr, ages, units);
        var heroAbilities = CreateHeroAbilities(mapper, gdr);
        var heroAbilityComponents = CreateHeroAbilityComponents(mapper, gdr);
        var treasureHuntBattles = CreateTreasureHuntBattles(mapper, gdr, units);
        var anubisAwakeningEncounters = CreateAnubisAwakeningBattles(mapper,
            gdr.RegionDefinitions.First(x => x.Region.EndsWith(nameof(RegionId.AncientEgyptDungeon))).Encounters, gdr,
            units);
        var wonders = CreateWonders(mapper, gdr);
        var heroAwakeningComponents = CreateHeroAwakeningComponents(mapper, gdr);
        var cities = CreateCities(mapper, gdr);
        var buildingCustomizations = BuildingCustomizations(mapper, gdr, ages, units);
        var relics = mapper.Map<IList<Relic>>(gdr.RelicDefinitions);
        var heroes =
            mapper.Map<IList<Hero>>(gdr.HeroDefinitions.Where(h => !HeroesToSkip.Contains(h.Id)));
        var heroStarUps = mapper.Map<IList<Hero>>(gdr.HeroStarUpDefinitions.Where(h => !HeroesToSkip.Contains(h.Id)));
        var allHeroes = heroes.Concat(heroStarUps).ToList();
        var equipmentSets = mapper.Map<IList<EquipmentSetDefinition>>(gdr.EquipmentSetDefinitions);
        var legacyHeroes = allHeroes
            .Where(h => CurrentToLegacyHeroUnitIdMap.ContainsKey(h.UnitId))
            .Select(h => new Hero
            {
                Id = h.Id,
                UnitId = CurrentToLegacyHeroUnitIdMap[h.UnitId],
                AbilityId = h.AbilityId,
                AwakeningId = h.AwakeningId,
                ClassId = h.ClassId,
                ProgressionComponent = h.ProgressionComponent,
                SupportUnitType = h.SupportUnitType,
            })
            .ToList();
        var legacyUnits = units
            .Where(kvp => CurrentToLegacyHeroUnitIdMap.ContainsKey(kvp.Key))
            .Select(kvp => new Unit
            {
                Id = CurrentToLegacyHeroUnitIdMap[kvp.Key],
                Name = kvp.Value.Name,
                Stats = kvp.Value.Stats,
                Color = kvp.Value.Color,
                RarityId = kvp.Value.RarityId,
                Type = kvp.Value.Type,
            })
            .ToList();
        var expansionCosts = CreateExpansionCosts(mapper, gdr);
        var data = new Data
        {
            Worlds = worlds.AsReadOnly(),
            Buildings = buildings.AsReadOnly(),
            Units = units.Values,
            Heroes = allHeroes,
            ProgressionCosts = mapper.Map<IReadOnlyCollection<HeroProgressionCost>>(gdr.HeroProgressionCostDefinitions),
            AscensionCosts =
                mapper.Map<IReadOnlyCollection<HeroAscensionCost>>(gdr.HeroProgressionAscensionCostDefinitions),
            UnitBattleConstants = mapper.Map<UnitBattleConstants>(gdr.HeroBattleConstantsDefinition),
            UnitStatFormulaData =
                mapper.Map<IReadOnlyCollection<UnitStatFormulaData>>(gdr.HeroUnitStatFormulaDefinitions),
            TreasureHuntBattles = treasureHuntBattles.AsReadOnly(),
            HeroAbilities = heroAbilities.AsReadOnly(),
            HeroBattleAbilityComponents = heroAbilityComponents.AsReadOnly(),
            Wonders = wonders.Where(x => x.CityId != CityId.Capital).ToList().AsReadOnly(),
            HeroAwakeningComponents = heroAwakeningComponents.AsReadOnly(),
            Expansions = CreateExpansions(mapper, gdr).AsReadOnly(),
            Technologies = technologies.AsReadOnly(),
            Ages = ages.Select(kvp => kvp.Value).ToList(),
            Cities = cities.ToList(),
            BuildingCustomizations = buildingCustomizations.ToList(),
            HeroUnitTypes = mapper.Map<IReadOnlyCollection<HeroUnitType>>(gdr.HeroUnitTypeDefinitions),
            Resources = resources.AsReadOnly(),
            Relics = relics.AsReadOnly(),
            RelicBoostAgeModifiers = mapper.Map<IDictionary<string, float>>(gdr.RelicBoostAgeModifiers).AsReadOnly(),
            EquipmentSetDefinitions = equipmentSets.AsReadOnly(),
            LegacyHeroes = legacyHeroes.AsReadOnly(),
            LegacyUnits = legacyUnits.AsReadOnly(),
            BattleEventRegions = new Dictionary<RegionId, IReadOnlyCollection<BattleEventEncounter>>
            {
                [RegionId.AncientEgyptDungeon] = anubisAwakeningEncounters.AsReadOnly(),
            },
            ExpansionCosts = expansionCosts.AsReadOnly(),
        };

        return data;
    }

    private static IList<BattleEventEncounter> CreateAnubisAwakeningBattles(IMapper mapper,
        IEnumerable<string> encounterIds, GameDesignResponse gdr, Dictionary<string, Unit> units)
    {
        var encounters = gdr.EncounterDefinitions.Where(x => encounterIds.Contains(x.EncounterId)).ToList();
        return
            mapper.Map<IList<BattleEventEncounter>>(encounters, opt =>
            {
                opt.Items.Add(ContextKeys.BATTLES_DEFINITIONS, gdr.HeroBattleDefinitions);
                opt.Items.Add(ContextKeys.BATTLE_WAVES_DEFINITIONS, gdr.HeroBattleWaveDefinitions);
                opt.Items.Add(ContextKeys.UNITS, units);
            });
    }
}
