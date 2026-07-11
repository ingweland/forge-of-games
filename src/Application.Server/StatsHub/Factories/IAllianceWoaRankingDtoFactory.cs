using Ingweland.Fog.Dtos.Hoh.Stats;
using Ingweland.Fog.Models.Fog.Entities;

namespace Ingweland.Fog.Application.Server.StatsHub.Factories;

public interface IAllianceWoaRankingDtoFactory
{
    AllianceWoaRankingDto Create(AllianceWoaRanking entity, InGameEventEntity inGameEvent);
}
