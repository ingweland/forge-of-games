using Ingweland.Fog.Application.Client.Web.StatsHub.Abstractions;
using Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;
using Ingweland.Fog.Dtos.Hoh;
using Ingweland.Fog.Dtos.Hoh.Stats;

namespace Ingweland.Fog.Application.Client.Web.StatsHub.Factories;

public class AllianceWoaRankingViewModelFactory : IAllianceWoaRankingViewModelFactory
{
    public AllianceWoaRankingViewModel Create(AllianceWoaRankingDto dto, WoaTierDto tier)
    {
        return new AllianceWoaRankingViewModel
        {
            DivisionId = dto.DivisionId,
            EventLabel = $"{dto.StartedAt:d} - {dto.EndedAt:d}",
            EloRatingFormatted = dto.EloRating.ToString("N0"),
            EloDelta = dto.EloDelta,
            VictoryPointsFormatted = dto.VictoryPoints.ToString("N0"),
            Tier = tier,
            CurrentVictoryPointsShareFormatted = dto.CurrentVictoryPointsShare?.ToString("P1"),
            ExpectedVictoryPointsShareFormatted = dto.ExpectedVictoryPointsShare?.ToString("P1"),
            VictoryPointsShareComparison =
                dto.CurrentVictoryPointsShare.HasValue && dto.ExpectedVictoryPointsShare.HasValue
                    ? dto.CurrentVictoryPointsShare.Value.CompareTo(dto.ExpectedVictoryPointsShare.Value)
                    : 0,
        };
    }
}
