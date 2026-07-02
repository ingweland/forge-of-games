using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.Extensions;
using Ingweland.Fog.Application.Client.Web.Factories.Interfaces;
using Ingweland.Fog.Application.Client.Web.Providers.Interfaces;
using Ingweland.Fog.Application.Client.Web.ViewModels.Hoh.Battle;
using Ingweland.Fog.Application.Core.Calculators.Interfaces;
using Ingweland.Fog.Application.Core.Helpers;
using Ingweland.Fog.Application.Core.Interfaces;
using Ingweland.Fog.Dtos.Hoh.Units;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Battle;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.Shared.Helpers;
using Microsoft.Extensions.Localization;

namespace Ingweland.Fog.Application.Client.Web.Factories;

public class BattleWaveSquadViewModelFactory(
    IAssetUrlProvider assetUrlProvider,
    IUnitPowerCalculator unitPowerCalculator,
    IStringLocalizer<FogResource> loc,
    IHeroValidator heroValidator) : IBattleWaveSquadViewModelFactory
{
    public IEnumerable<BattleWaveSquadViewModel> Create(IReadOnlyCollection<BattleWaveSquad> squads,
        IReadOnlyCollection<UnitDto> units, IReadOnlyCollection<HeroDto> heroes)
    {
        if (squads == null || squads.Count == 0)
        {
            throw new ArgumentException("Squads collection cannot be null or empty.", nameof(squads));
        }

        var firstSquad = squads.First();
        var expectedUnitId = firstSquad.Unit.UnitId;
        if (squads.Any(s => s.Unit.UnitId != expectedUnitId))
        {
            throw new InvalidOperationException($"All squads must have the same unit id.");
        }

        var result = new List<BattleWaveSquadViewModel>();
        var unit = units.First(u => u.Id == firstSquad.Unit.UnitId);
        if (firstSquad.Hero != null)
        {
            result.AddRange(squads.Select(squad => CreateInternal(squad, 1, unit, heroes)));
        }
        else
        {
            var total = squads.Sum(s => GetSquadSize(s, unit));
            result.Add(CreateInternal(firstSquad, total, unit, heroes));
        }

        return result;
    }

    public BattleWaveSquadViewModel Create(BattleWaveSquad squad, IReadOnlyCollection<UnitDto> units,
        IReadOnlyCollection<HeroDto> heroes)
    {
        var unit = units.First(u => u.Id == squad.Unit.UnitId);
        var size = GetSquadSize(squad, unit);

        return CreateInternal(squad, size, unit, heroes);
    }

    private int GetSquadSize(BattleWaveSquad squad, UnitDto unit)
    {
        var size = 1;
        if (squad.SupportUnit != null)
        {
            size = (int) unit.Stats.First(x => x.Type == UnitStatType.SquadSize).Value;
            if (squad.SupportUnit.UnitStatsOverrides.TryGetValue(UnitStatType.SquadSize, out var overridenSize))
            {
                size = (int) overridenSize;
            }
        }

        return size;
    }

    private BattleWaveSquadViewModel CreateInternal(BattleWaveSquad squad, int squadSize,
        UnitDto unit, IReadOnlyCollection<HeroDto> heroes)
    {
        var level = $"{loc[FogResource.Hoh_Lvl]} {squad.Unit.Level}";
        double power = 0;
        if (squad.SupportUnit != null)
        {
            power = unitPowerCalculator.CalculateUnitPower(unit, squad.SupportUnit.Level, squadSize);
        }

        //TODO: get concrete star class
        if (squad.Hero != null && heroValidator.IsValidHero(squad.Hero.UnitId))
        {
            var hero = heroes.First(u => u.Unit.Id == squad.Hero.UnitId);
            // TODO: start using squad.Hero.AbilityLevel once they fix the data
            var abilityLvl = int.Parse(HohStringParser.GetConcreteId(squad.Hero.Abilities.First(), '_'));
            squad.Hero.AbilityLevel = abilityLvl;
            level += $" | {loc[FogResource.Hoh_Hero_AbilityLvl]} {abilityLvl}";
            power = unitPowerCalculator.CalculateHeroPower(hero.Unit, hero.StarClass, squad.Hero.Level,
                squad.Hero.AscensionLevel, abilityLvl);
        }

        UnitColorAffinity? colorAffinity = null;
        if (UnitColorAdvantageSystem.ColorAffinities.TryGetValue(unit.Color, out var unitColorAffinity))
        {
            colorAffinity = unitColorAffinity;
        }

        return new BattleWaveSquadViewModel
        {
            Name = unit.Name,
            Amount = squadSize.ToString(),
            Color = unit.Color,
            Level = level,
            ImageUrl = assetUrlProvider.GetHohUnitPortraitUrl(unit.AssetId),
            TypeIconUrl = assetUrlProvider.GetHohIconUrl(unit.Type.GetTypeIconId()),
            Hero = squad.Hero,
            Power = power,
            ColorAffinity = colorAffinity,
        };
    }

    
}
