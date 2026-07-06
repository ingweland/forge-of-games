using Ingweland.Fog.Application.Core.CityPlanner.Abstractions;
using Ingweland.Fog.Application.Core.CityPlanner.Stats.BuildingTypedStats;
using Ingweland.Fog.Models.Hoh.Entities.City;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Core.CityPlanner.Stats;

public class StatsProcessor(
    CityMapStateCore cityMapState,
    IProductionStatsProcessor productionStatsProcessor,
    IReadOnlyCollection<MapAreaHappinessProvider> mapAreaHappinessProviders)
{
    private void UpdateHappiness()
    {
        foreach (var target in cityMapState.HappinessConsumers)
        {
            UpdateHappiness(target);
        }
    }

    private void UpdateHappiness(CityMapEntity target)
    {
        var happinessConsumer = target.FirstOrDefaultStat<HappinessConsumer>();
        if (happinessConsumer == null)
        {
            return;
        }

        var intersections = cityMapState.HappinessProviders
            .Where(hp => hp is {IsLocked: false, IsUpgrading: false})
            .Where(hp => !hp.ExcludeFromStats && target.Bounds.IntersectsWith(hp.OverflowBounds!.Value)).ToList();
        var age = cityMapState.CityAge;
        var happiness = intersections.Sum(cme =>
            cityMapState.Buildings[cme.CityEntityId].CultureComponent!.GetValue(age.Id, cme.Level));
        var mapIntersections = mapAreaHappinessProviders.Where(hp => target.Bounds.IntersectsWith(hp.Bounds)).ToList();
        var mapHappiness = mapIntersections.Sum(src => src.Value);
        happinessConsumer.ConsumedHappiness = happiness + mapHappiness;
        target.HappinessFraction =
            (float) happinessConsumer.ConsumedHappiness /
            cityMapState.Buildings[target.CityEntityId].BuffDetails!.Value;
    }

    public CityStats UpdateStats()
    {
        UpdateEvolvingBuildings();
        UpdateHappiness();
        UpdateProduction();
        return CityStatsProcessor.Update(cityMapState.CityMapEntities.Values, mapAreaHappinessProviders,
            cityMapState.OpenExpansions, cityMapState.PremiumExpansionCosts, GetWonderWorkers(), GetWonderModifiers(),
            cityMapState.PremiumExpansions.Count);
    }

    private void UpdateEvolvingBuildings()
    {
        var evolvingBuildings = cityMapState.TypedEntities.Where(kvp => kvp.Key == BuildingType.Evolving)
            .SelectMany(kvp => kvp.Value);
        foreach (var cme in evolvingBuildings)
        {
            cme.FirstOrDefaultStat<HappinessProvider>()?.Update(cityMapState.CityAge.Id, cme.Level);
        }
    }

    private void UpdateProduction()
    {
        foreach (var cme in cityMapState.CityMapEntities.Values)
        {
            var modifiers = GetWonderModifiers();
            if (modifiers == null)
            {
                productionStatsProcessor.UpdateProduction(cme);
            }
            else
            {
                productionStatsProcessor.UpdateProduction(cme, modifiers);
            }
        }
    }

    private IReadOnlyDictionary<string, double>? GetWonderModifiers()
    {
        var boostComponent = cityMapState.CityWonder?.Components.OfType<BoostResourceComponent>().ToList();
        return boostComponent?.Count > 0
            ? boostComponent.ToDictionary(src => src.ResourceId!, src => src.GetValue(cityMapState.CityWonderLevel))
            : null;
    }

    private IReadOnlyDictionary<WorkerType, int>? GetWonderWorkers()
    {
        return cityMapState.CityWonder?.Components.OfType<GrantWorkerComponent>()
            .ToDictionary(x => x.WorkerType, x => x.GetWorkerCount(cityMapState.CityWonderLevel));
    }

    public CityStats UpdateStats(CityMapEntity target)
    {
        var building = cityMapState.Buildings[target.CityEntityId];
        switch (building.Type)
        {
            case BuildingType.Home:
            case BuildingType.Farm:
            case BuildingType.Barracks:
            case BuildingType.Workshop:
            case BuildingType.Aviary:
            case BuildingType.Quarry:
            case BuildingType.PapyrusField:
            case BuildingType.GoldMine:
            case BuildingType.Merchant:
            case BuildingType.CamelFarm:
            case BuildingType.Plantation:
            {
                UpdateHappiness(target);
                var modifiers = GetWonderModifiers();
                if (modifiers == null)
                {
                    productionStatsProcessor.UpdateProduction(target);
                }
                else
                {
                    productionStatsProcessor.UpdateProduction(target, modifiers);
                }

                break;
            }
            case BuildingType.Collectable:
            case BuildingType.CultureSite:
            case BuildingType.RitualSite:
            case BuildingType.Irrigation:
            case BuildingType.PresetIrrigation:
            {
                UpdateHappiness();
                UpdateProduction();
                break;
            }
            case BuildingType.CityHall:
            case BuildingType.Evolving:
            {
                UpdateEvolvingBuildings();
                UpdateHappiness();
                UpdateProduction();
                break;
            }
            case BuildingType.ExtractionPoint:
            case BuildingType.RiceFarm:
            case BuildingType.FishingPier:
            case BuildingType.Beehive:
            case BuildingType.Pier:
            {
                var modifiers = GetWonderModifiers();
                if (modifiers == null)
                {
                    productionStatsProcessor.UpdateProduction(target);
                }
                else
                {
                    productionStatsProcessor.UpdateProduction(target, modifiers);
                }

                break;
            }
        }

        return CityStatsProcessor.Update(cityMapState.CityMapEntities.Values, mapAreaHappinessProviders,
            cityMapState.OpenExpansions, cityMapState.PremiumExpansionCosts, GetWonderWorkers(), GetWonderModifiers(),
            cityMapState.PremiumExpansions.Count);
    }
}
