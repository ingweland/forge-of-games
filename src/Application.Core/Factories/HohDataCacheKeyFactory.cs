using System.Globalization;
using Ingweland.Fog.Application.Core.Factories.Interfaces;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Core.Factories;

public class HohDataCacheKeyFactory : IHohDataCacheKeyFactory
{
    public string HeroDto(string heroId, Guid version)
    {
        return $"hero_dto:{heroId}:{CultureInfo.CurrentCulture.Name}:{version}";
    }

    public string BattleEventRegionDto(RegionId regionId, Guid version)
    {
        return $"battle_event_region_dto:{regionId}:{CultureInfo.CurrentCulture.Name}:{version}";
    }

    public string RelicDtos(Guid version)
    {
        return $"relic_dtos:{CultureInfo.CurrentCulture.Name}:{version}";
    }

    public string HeroesBasicData(Guid version)
    {
        return $"heroes-basic-data:{CultureInfo.CurrentCulture.Name}:{version}";
    }

    public string HohAges(Guid version)
    {
        return $"hoh-ages:{CultureInfo.CurrentCulture.Name}:{version}";
    }

    public string PvpTiers(Guid version)
    {
        return $"pvp-tiers:{CultureInfo.CurrentCulture.Name}:{version}";
    }

    public string TreasureHuntLeagues(Guid version)
    {
        return $"ath-leagues:{CultureInfo.CurrentCulture.Name}:{version}";
    }

    public string WoaTiers(Guid version)
    {
        return $"woa-tiers:{CultureInfo.CurrentCulture.Name}:{version}";
    }

    public string HohResources(Guid version)
    {
        return $"hoh-resources:{CultureInfo.CurrentCulture.Name}:{version}";
    }
}
