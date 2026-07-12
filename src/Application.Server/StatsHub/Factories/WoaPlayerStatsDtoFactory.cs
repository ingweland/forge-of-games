using Ingweland.Fog.Dtos.Hoh.Stats;
using Ingweland.Fog.Models.Fog.Entities;

namespace Ingweland.Fog.Application.Server.StatsHub.Factories;

public class WoaPlayerStatsDtoFactory : IWoaPlayerStatsDtoFactory
{
    public WoaPlayerStatsDto Create(WoaPlayerStatsEntity entity, InGameEventEntity inGameEvent)
    {
        return new WoaPlayerStatsDto
        {
            ContributionPoints = entity.ContributionPoints,
            HealingDone = entity.HealingDone,
            LostAttacks = entity.LostAttacks,
            RepairsStarted = entity.RepairsStarted,
            VictoryPoints = entity.VictoryPoints,
            WonAttacks = entity.WonAttacks,
            WonDefenses = entity.WonDefenses,
            StartedAt = inGameEvent.StartAt,
            EndedAt = inGameEvent.EndAt,
        };
    }
}
