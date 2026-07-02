using AutoMapper;
using Ingweland.Fog.Application.Core.Factories.Interfaces;
using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Ingweland.Fog.Application.Core.Services.Hoh.Abstractions;
using Ingweland.Fog.Dtos.Hoh.Battle;
using Ingweland.Fog.Dtos.Hoh.Units;
using Ingweland.Fog.Models.Hoh.Enums;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Application.Core.Services.Hoh;

public class CampaignService(
    IHohCoreDataRepository hohCoreDataRepository,
    IRegionDtoFactory regionDtoFactory,
    IUnitDtoFactory unitDtoFactory,
    IUnitService unitService,
    IMapper mapper,
    IHohGameLocalizationService gameLocalizationService,
    ILogger<CampaignService> logger) : ICampaignService
{
    public async Task<IReadOnlyCollection<ContinentBasicDto>> GetCampaignContinentsBasicDataAsync()
    {
        var world = await hohCoreDataRepository.GetWorldAsync(WorldId.Starter);
        if (world == null)
        {
            logger.LogError($"Failed to get campaign continents by WorldId: {WorldId.Starter}");
            return Array.Empty<ContinentBasicDto>();
        }

        var result = world.Continents.OrderBy(c => c.Index)
            .Select(mapper.Map<ContinentBasicDto>).ToList();
        return result.AsReadOnly();
    }

    public async Task<RegionDto?> GetRegionAsync(RegionId regionId)
    {
        var region = (await hohCoreDataRepository.GetWorldAsync(WorldId.Starter))?.Continents
            .SelectMany(c => c.Regions)
            .FirstOrDefault(r => r.Id == regionId);
        if (region == null)
        {
            logger.LogError($"Failed to get region by RegionId: {regionId}");
            return null;
        }

        var unitIds = region.Encounters.SelectMany(e =>
            e.Details.Values.Where(ed => ed.BattleDetails != null).SelectMany(ed =>
                ed.BattleDetails!.Waves.SelectMany(bw =>
                    bw.Squads.Select(bws => (
                        UnitId: bws.Hero != null ? bws.Hero.UnitId : bws.SupportUnit!.UnitId,
                        IsHero: bws.Hero != null))))).ToHashSet();
        var units = new List<UnitDto>();
        var heroes = new List<HeroDto>();
        foreach (var t in unitIds)
        {
            var unit = await hohCoreDataRepository.GetUnitAsync(t.UnitId);
            if (unit == null)
            {
                logger.LogError($"Failed to get unit by UnitId: {t.UnitId}");
                return null;
            }

            units.Add(unitDtoFactory.Create(unit, await hohCoreDataRepository.GetUnitStatFormulaData(),
                await hohCoreDataRepository.GetUnitBattleConstants(),
                await hohCoreDataRepository.GetHeroUnitType(unit.Type)));

            if (t.IsHero)
            {
                var hero = await unitService.GetHeroAsync(t.UnitId);
                if (hero != null)
                {
                    heroes.Add(hero);
                }
            }
        }

        return regionDtoFactory.Create(region, units, heroes);
    }

    public Task<RegionBasicDto> GetRegionBasicDataAsync(RegionId regionId)
    {
        return Task.FromResult(new RegionBasicDto
        {
            Id = regionId,
            Name = gameLocalizationService.GetRegionName(regionId),
        });
    }

    public async Task<IReadOnlyCollection<BattleEventBasicDto>> GetBattleEventsBasicDataAsync()
    {
        var result = new List<BattleEventBasicDto>();
        var ancientEgyptRegion = (await hohCoreDataRepository.GetWorldAsync(WorldId.AncientEgyptDungeon))?.Continents
            .SelectMany(c => c.Regions)
            .FirstOrDefault(r => r.Id == RegionId.AncientEgyptDungeon);
        if (ancientEgyptRegion != null)
        {
            result.Add(new BattleEventBasicDto
            {
                Id = RegionId.AncientEgyptDungeon,
                EncounterCount = ancientEgyptRegion.Encounters.Count,
                EncounterStartIndex = ancientEgyptRegion.Encounters.MinBy(x => x.Index)!.Index,
                Name = gameLocalizationService.GetBattleEventName("BattleEvent_AncientEgyptEvent_AnubisDungeon"),
            });
        }
        else
        {
            logger.LogWarning($"Failed to get region by RegionId: {RegionId.AncientEgyptDungeon}");
        }

        var scyllaDungeonRegion = (await hohCoreDataRepository.GetWorldAsync(WorldId.ScyllaDungeon))?.Continents
            .SelectMany(c => c.Regions)
            .FirstOrDefault(r => r.Id == RegionId.ScyllaDungeon);
        if (scyllaDungeonRegion != null)
        {
            result.Add(new BattleEventBasicDto
            {
                Id = RegionId.ScyllaDungeon,
                EncounterCount = scyllaDungeonRegion.Encounters.Count,
                EncounterStartIndex = scyllaDungeonRegion.Encounters.MinBy(x => x.Index)!.Index,
                Name = gameLocalizationService.GetBattleEventName("BattleEvent_OdysseyEvent_ScyllaDungeon"),
            });
        }
        else
        {
            logger.LogWarning($"Failed to get region by RegionId: {RegionId.ScyllaDungeon}");
        }

        return result;
    }

    public async Task<BattleEventRegionDto?> GetBattleEventRegionAsync(RegionId regionId)
    {
        var encounters = await hohCoreDataRepository.GetBattleEventRegionAsync(regionId);
        if (encounters.Count == 0)
        {
            logger.LogWarning($"Failed to get battle event region by RegionId: {regionId}");
            return null;
        }

        var unitIds = encounters.SelectMany(e =>
            e.BattleDetails.Waves.SelectMany(bw =>
                bw.Squads.Select(bws => (
                    UnitId: bws.Hero != null ? bws.Hero.UnitId : bws.SupportUnit!.UnitId,
                    IsHero: bws.Hero != null)))).ToHashSet();
        var units = new List<UnitDto>();
        var heroes = new List<HeroDto>();
        foreach (var t in unitIds)
        {
            var unit = await hohCoreDataRepository.GetUnitAsync(t.UnitId);
            if (unit == null)
            {
                logger.LogWarning($"Failed to get unit by UnitId: {t.UnitId}");
                return null;
            }

            units.Add(unitDtoFactory.Create(unit, await hohCoreDataRepository.GetUnitStatFormulaData(),
                await hohCoreDataRepository.GetUnitBattleConstants(),
                await hohCoreDataRepository.GetHeroUnitType(unit.Type)));

            if (t.IsHero)
            {
                var hero = await unitService.GetHeroAsync(t.UnitId);
                if (hero != null)
                {
                    heroes.Add(hero);
                }
            }
        }

        return new BattleEventRegionDto
        {
            Id = RegionId.AncientEgyptDungeon,
            Name = gameLocalizationService.GetBattleEventName("BattleEvent_AncientEgyptEvent_AnubisDungeon"),
            Encounters = mapper.Map<IReadOnlyCollection<BattleEventEncounterDto>>(encounters.OrderBy(e => e.Index)),
            Units = units,
            Heroes = heroes,
        };
    }
}
