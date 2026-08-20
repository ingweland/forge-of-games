using AutoMapper;
using Ingweland.Fog.Application.Core.Factories.Interfaces;
using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Dtos.Hoh.Battle;
using Ingweland.Fog.Dtos.Hoh.Units;
using Ingweland.Fog.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Application.Core.Services.Hoh;

public class TreasureHuntService(
    IHohCoreDataRepository hohCoreDataRepository,
    ITreasureHuntStageDtoFactory treasureHuntStageDtoFactory,
    IUnitDtoFactory unitDtoFactory,
    IUnitService unitService,
    IMapper mapper,
    ILogger<TreasureHuntService> logger) : ITreasureHuntService
{
    public async Task<IReadOnlyCollection<TreasureHuntDifficultyDataBasicDto>> GetDifficultiesAsync()
    {
        var difficulties = await hohCoreDataRepository.GetTreasureHuntDifficultiesAsync();
        return mapper.Map<IReadOnlyCollection<TreasureHuntDifficultyDataBasicDto>>(difficulties);
    }

    public async Task<TreasureHuntStageDto?> GetStageAsync(int difficulty, int stageIndex)
    {
        var stage = await hohCoreDataRepository.GetTreasureHuntStageAsync(difficulty, stageIndex);
        if (stage == null)
        {
            return null;
        }

        var unitIds = stage.Battles.SelectMany(e =>
                e.Waves.SelectMany(bw =>
                    bw.Squads.Select(bws =>
                        bws.Hero != null ? bws.Hero.UnitId : bws.SupportUnit!.UnitId)))
            .ToHashSet();
        var units = new List<UnitDto>();
        var heroes = new List<HeroDto>();
        foreach (var unitId in unitIds)
        {
            var unit = await hohCoreDataRepository.GetUnitAsync(unitId);
            if (unit == null)
            {
                logger.LogError($"Failed to get unit by UnitId: {unitId}");
                return null;
            }

            units.Add(unitDtoFactory.Create(unit, await hohCoreDataRepository.GetUnitStatFormulaData(),
                await hohCoreDataRepository.GetUnitBattleConstants(),
                await hohCoreDataRepository.GetHeroUnitType(unit.Type)));

            var hero = await unitService.GetHeroAsync(unitId);
            if (hero != null)
            {
                heroes.Add(hero);
            }
        }

        return treasureHuntStageDtoFactory.Create(stage, difficulty, units, heroes);
    }

    public async Task<IReadOnlyCollection<TreasureHuntEncounterBasicDataDto>> GetTreasureHuntEncountersBasicDataAsync()
    {
        var encounters = new List<TreasureHuntEncounterBasicDataDto>();
        var difficulties = await hohCoreDataRepository.GetTreasureHuntDifficultiesAsync();
        foreach (var difficulty in difficulties)
        {
            foreach (var stageData in difficulty.Stages)
            {
                encounters.AddRange(stageData.Battles.Select(b =>
                    {
                        var concreteId = HohStringParser.GetConcreteId(b.Id);
                        var parts = concreteId.Split('_');
                        return int.Parse(parts[3]);
                    })
                    .Order()
                    .Select((src, index) => new TreasureHuntEncounterBasicDataDto
                    {
                        Difficulty = difficulty.Difficulty,
                        Stage = stageData.Index,
                        Encounter = src,
                        Index = index,
                    }));
            }
        }

        return encounters;
    }
}
