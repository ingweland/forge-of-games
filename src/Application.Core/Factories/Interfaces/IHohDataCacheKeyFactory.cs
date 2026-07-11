using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Core.Factories.Interfaces;

public interface IHohDataCacheKeyFactory
{
    string HeroDto(string heroId, Guid version);
    string BattleEventRegionDto(RegionId regionId, Guid version);
    string RelicDtos(Guid version);
    string HeroesBasicData(Guid version);
    string HohAges(Guid version);
    string PvpTiers(Guid version);
    string TreasureHuntLeagues(Guid version);
    string WoaTiers(Guid version);
    string HohResources(Guid version);
}
