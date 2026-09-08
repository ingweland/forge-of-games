using System.Collections.ObjectModel;
using Ingweland.Fog.Application.Core.Factories.Interfaces;
using Ingweland.Fog.Application.Core.Interfaces;
using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Dtos.Hoh.Units;
using Ingweland.Fog.Models.Hoh.Entities.Units;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Application.Core.Services.Hoh;

public class UnitService(
    IHohCoreDataRepository hohCoreDataRepository,
    ILogger<UnitService> logger,
    IHeroBasicDtoFactory heroBasicDtoFactory,
    IHeroDtoFactory heroDtoFactory,
    IHeroAbilityDtoFactory heroAbilityDtoFactory,
    IHohDataCache dataCache,
    IHohDataCacheKeyFactory cacheKeyFactory,
    IHeroAbilityService heroAbilityService)
    : IUnitService
{
    private static readonly HashSet<string> LegacyUnitIds =
    [
        "unit.Unit_WilliamTell_5",
    ];

    private ReadOnlyDictionary<string, ReadOnlySet<string>>? _heroAbilityTags;

    public Task<HeroDto?> GetHeroAsync(string id)
    {
        var version = hohCoreDataRepository.Version;
        return dataCache.GetOrAddAsync(cacheKeyFactory.HeroDto(id, version), () => CreateHeroAsync(id), version);
    }

    public Task<IReadOnlyCollection<HeroBasicDto>> GetHeroesBasicDataAsync()
    {
        var version = hohCoreDataRepository.Version;
        return dataCache.GetOrAddAsync(cacheKeyFactory.HeroesBasicData(version), CreateHeroesBasicDataAsync, version);
    }

    private async Task FetchHeroAbilityTagsAsync()
    {
        if (_heroAbilityTags != null)
        {
            return;
        }

        var abilityTags = await heroAbilityService.GetHeroAbilityFeaturesAsync();
        _heroAbilityTags = abilityTags.ToDictionary(x => x.HeroId, x => new ReadOnlySet<string>(x.Tags)).AsReadOnly();
    }

    private async Task<IReadOnlyCollection<HeroBasicDto>> CreateHeroesBasicDataAsync()
    {
        await FetchHeroAbilityTagsAsync();
        var heroes = new List<HeroBasicDto>();
        foreach (var hero in await hohCoreDataRepository.GetHeroesAsync())
        {
            var unit = await hohCoreDataRepository.GetUnitAsync(hero.UnitId);
            if (unit == null)
            {
                logger.LogError($"Could not find unit {hero.UnitId} for the hero with id {hero.Id}");
                continue;
            }

            var heroAbilityTags = _heroAbilityTags!.GetValueOrDefault(hero.Id, ReadOnlySet<string>.Empty);
            heroes.Add(heroBasicDtoFactory.Create(hero, unit, heroAbilityTags));
        }

        return heroes.OrderBy(h => h.Name).ToList().AsReadOnly();
    }

    private async Task<HeroDto?> CreateHeroAsync(string id)
    {
        Hero? hero;
        if (!LegacyUnitIds.Contains(id))
        {
            hero = await hohCoreDataRepository.GetHeroAsync(id) ??
                await hohCoreDataRepository.GetHeroByUnitIdAsync(id);
        }
        else
        {
            hero = await hohCoreDataRepository.GetHeroByLegacyUnitIdAsync(id);
        }

        if (hero == null)
        {
            logger.LogDebug($"Could not find hero with id {id}");
            return null;
        }

        Unit? unit;
        if (!LegacyUnitIds.Contains(id))
        {
            unit = await hohCoreDataRepository.GetUnitAsync(hero.UnitId);
        }
        else
        {
            unit = await hohCoreDataRepository.GetUnitByLegacyUnitIdAsync(hero.UnitId);
        }

        if (unit == null)
        {
            logger.LogError($"Could not find unit {hero.UnitId} for the hero with id {id}");
            return null;
        }

        var progressionCosts =
            await hohCoreDataRepository.GetHeroProgressionCostsAsync(hero.ProgressionComponent.CostId);
        if (progressionCosts == null)
        {
            logger.LogError($"Could not find progression costs {hero.ProgressionComponent.CostId
            } for the hero with id {
                id}");
            return null;
        }

        var ascensionCosts =
            await hohCoreDataRepository.GetHeroAscensionCostsAsync(hero.ProgressionComponent.AscensionCostId);
        if (ascensionCosts == null)
        {
            logger.LogError(
                $"Could not find ascension progression costs {hero.ProgressionComponent.AscensionCostId
                } for the hero with id {
                    id}");
            return null;
        }

        var awakeningComponent =
            await hohCoreDataRepository.GetHeroAwakeningComponentAsync(hero.AwakeningId);
        if (awakeningComponent == null)
        {
            logger.LogError(
                $"Could not find awakening component {hero.AwakeningId} for the hero with id {id}");
            return null;
        }

        var ability = await GetHeroAbilityAsync(hero);
        if (ability == null)
        {
            return null;
        }

        var sortedProgressionCosts = progressionCosts.LevelCosts.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value)
            .ToList()
            .AsReadOnly();
        var sortedAscensionCosts = ascensionCosts.LevelCosts.OrderBy(kvp => kvp.Key).ToDictionary().AsReadOnly();

        var units = await hohCoreDataRepository.GetUnitsAsync(unit.Type);
        var baseSupportUnit = units.Single(u => u.Id.StartsWith("unit.Unit_StoneAge_Player"));

        return heroDtoFactory.Create(hero, unit, sortedProgressionCosts, sortedAscensionCosts,
            await hohCoreDataRepository.GetUnitStatFormulaData(),
            await hohCoreDataRepository.GetUnitBattleConstants(),
            baseSupportUnit,
            awakeningComponent,
            ability,
            await hohCoreDataRepository.GetHeroUnitType(unit.Type));
    }

    private async Task<HeroAbilityDto?> GetHeroAbilityAsync(Hero hero)
    {
        var abilityComponent = await hohCoreDataRepository.GetHeroAbilityComponentAsync(hero.AbilityId);
        if (abilityComponent == null)
        {
            logger.LogError($"Could not find ability component for hero with id {hero.Id} and hero ability id {
                hero.AbilityId}");
            return null;
        }

        var abilities = new List<BattleAbility>();
        foreach (var level in abilityComponent.Levels)
        {
            var ability = await hohCoreDataRepository.GetHeroAbilityAsync(level.AbilityId);
            if (ability == null)
            {
                logger.LogError($"Could not find ability for hero with id {hero.Id} and ability id {level.AbilityId}");
                return null;
            }

            abilities.Add(ability);
        }

        return heroAbilityDtoFactory.Create(abilityComponent, abilities);
    }
}
