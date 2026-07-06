using AutoMapper;
using Ingweland.Fog.Application.Core.CityPlanner.Abstractions;
using Ingweland.Fog.Application.Core.Extensions;
using Ingweland.Fog.Application.Core.Factories.Interfaces;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Entities.City;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Core.CityPlanner;

public class HohCityFactory(IMapper mapper, InitCityConfigs initCityConfigs, IHohCitySnapshotFactory snapshotFactory)
    : IHohCityFactory
{
    public HohCity CreateNewCapital(int cityPlannerVersion)
    {
        return Create(new NewCityRequest {Name = $"{CityId.Capital}", CityId = CityId.Capital},
            cityPlannerVersion);
    }

    public HohCity Create(NewCityRequest newCityRequest, int cityPlannerVersion)
    {
        return new HohCity
        {
            Id = Guid.NewGuid().ToString(),
            InGameCityId = newCityRequest.CityId,
            AgeId = newCityRequest.CityId.ToDefaultAge(),
            Entities = initCityConfigs.GetMapEntities(
                GetInitEntitiesKey(newCityRequest.CityId, newCityRequest.WonderId)),
            Name = newCityRequest.Name,
            UnlockedExpansions = initCityConfigs.GetExpansions(newCityRequest.CityId),
            WonderId = newCityRequest.WonderId,
            WonderLevel = newCityRequest.WonderLevel,
            UpdatedAt = DateTime.Now.ToLocalTime(),
            CityPlannerVersion = cityPlannerVersion,
        };
    }

    public HohCity Create(string id, CityId inGameCityId, string ageId, string name,
        IEnumerable<CityMapEntity> entities, IEnumerable<CityMapEntity> inventoryBuildings,
        IReadOnlyCollection<HohCitySnapshot> snapshots, IEnumerable<string> expansions,
        IEnumerable<string> premiumExpansions, int cityPlannerVersion, WonderId cityWonderId = WonderId.Undefined,
        int cityWonderLevel = 0)
    {
        return new HohCity
        {
            Id = id,
            InGameCityId = inGameCityId,
            AgeId = ageId,
            Entities = mapper.Map<IReadOnlyCollection<HohCityMapEntity>>(entities),
            InventoryBuildings = mapper.Map<IReadOnlyCollection<HohCityMapEntity>>(inventoryBuildings),
            Name = name,
            Snapshots = snapshots,
            UnlockedExpansions = expansions.ToHashSet(),
            WonderId = cityWonderId,
            WonderLevel = cityWonderLevel,
            UpdatedAt = DateTime.Now.ToLocalTime(),
            CityPlannerVersion = cityPlannerVersion,
            UnlockedPremiumExpansions = premiumExpansions.ToHashSet(),
        };
    }

    public HohCity Create(string id, CityId inGameCityId, string ageId, string name,
        IReadOnlyCollection<HohCityMapEntity> entities, HashSet<string> expansions,
        IEnumerable<string> premiumExpansions, int cityPlannerVersion, WonderId cityWonderId = WonderId.Undefined,
        int cityWonderLevel = 0)
    {
        return new HohCity
        {
            Id = id,
            InGameCityId = inGameCityId,
            AgeId = ageId,
            Entities = entities,
            InventoryBuildings = [],
            Name = name,
            Snapshots = [],
            UnlockedExpansions = expansions,
            WonderId = cityWonderId,
            WonderLevel = cityWonderLevel,
            UpdatedAt = DateTime.Now.ToLocalTime(),
            CityPlannerVersion = cityPlannerVersion,
            UnlockedPremiumExpansions = premiumExpansions.ToHashSet(),
        };
    }

    public HohCity Create(City inGameCity, IReadOnlyDictionary<string, Building> buildings, WonderId wonderId,
        int wonderLevel, string? name = null)
    {
        var cityHalls = buildings.Where(kvp => kvp.Value.Type == BuildingType.CityHall).Select(kvp => kvp.Value)
            .ToList();
        var cityHallMapEntity = inGameCity.MapEntities.Single(cme => cityHalls.Any(b => b.Id == cme.CityEntityId));
        var cityHall = cityHalls.First(b => b.Id == cityHallMapEntity.CityEntityId);
        var entities = mapper.Map<List<HohCityMapEntity>>(inGameCity.MapEntities);
        for (var i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];
            entity.Id = i;
            entity.Y = -entity.Y - (entity.IsRotated
                ? buildings[entity.CityEntityId].Width
                : buildings[entity.CityEntityId].Length);
        }

        AddAbsentLockedEntities(inGameCity.CityId, wonderId, entities);

        return new HohCity
        {
            Id = Guid.NewGuid().ToString(),
            InGameCityId = inGameCity.CityId,
            AgeId = cityHall.Age!.Id,
            Entities = entities.AsReadOnly(),
            Name = name ?? $"Import - {inGameCity.CityId} - {DateTime.Now:g}",
            Snapshots = new List<HohCitySnapshot> {snapshotFactory.Create(entities)},
            WonderId = wonderId,
            WonderLevel = wonderLevel,
            UpdatedAt = DateTime.UtcNow,
            UnlockedExpansions = inGameCity.OpenedExpansions.Select(src => src.Id).ToHashSet(),
            UnlockedPremiumExpansions = inGameCity.OpenedExpansions
                .Where(x => x.UnlockingType == ExpansionUnlockingType.Premium).Select(src => src.Id).ToHashSet(),
        };
    }

    public HohCity Create(OtherCity inGameCity, IReadOnlyDictionary<string, Building> buildings, string name)
    {
        var wonder = inGameCity.Wonders.FirstOrDefault();
        return Create(inGameCity, buildings, wonder?.Id ?? WonderId.Undefined, wonder?.Level ?? 0, name);
    }

    public HohCity Create(OtherCity inGameCity, IReadOnlyDictionary<string, Building> buildings)
    {
        var wonder = inGameCity.Wonders.FirstOrDefault();
        return Create(inGameCity, buildings, wonder?.Id ?? WonderId.Undefined, wonder?.Level ?? 0);
    }

    private static string GetInitEntitiesKey(CityId cityId, WonderId wonderId)
    {
        var key = cityId.ToString();
        if (cityId is CityId.Mayas_ChichenItza or CityId.Mayas_SayilPalace or CityId.Mayas_Tikal
            or CityId.Arabia_CityOfBrass or CityId.Arabia_NoriasOfHama or CityId.Arabia_Petra
            or CityId.AncientEgyptEvent or CityId.Ithaka)
        {
            key = wonderId.ToString();
        }

        return key;
    }

    private void AddAbsentLockedEntities(CityId cityId, WonderId wonderId, List<HohCityMapEntity> entities)
    {
        var initLockedEntities =
            initCityConfigs.GetMapEntities(GetInitEntitiesKey(cityId, wonderId)).Where(x => x.IsLocked);
        var i = entities.Count;
        foreach (var initLockedEntity in initLockedEntities)
        {
            if (entities.Any(x => x.X == initLockedEntity.X && x.Y == initLockedEntity.Y))
            {
                continue;
            }

            initLockedEntity.Id = i++;
            entities.Add(initLockedEntity);
        }
    }
}
