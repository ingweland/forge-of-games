using Ingweland.Fog.Application.Core.Interfaces;
using Ingweland.Fog.Dtos.Hoh.Stats;
using Ingweland.Fog.Models.Fog.Entities;

namespace Ingweland.Fog.Application.Server.StatsHub.Factories;

public class AllianceWoaRankingDtoFactory(IWoaTierHelper tierHelper) : IAllianceWoaRankingDtoFactory
{
    public AllianceWoaRankingDto Create(AllianceWoaRanking entity, InGameEventEntity inGameEvent)
    {
        return new AllianceWoaRankingDto
        {
            DivisionId = entity.DivisionId,
            EloDelta = entity.EloDelta,
            EloRating = entity.EloRating,
            VictoryPoints = entity.VictoryPoints,
            EventId = inGameEvent.Id,
            StartedAt = inGameEvent.StartAt,
            EndedAt = inGameEvent.EndAt,
            Tier = tierHelper.GetTier(entity.EloRating),
        };
    }
}
