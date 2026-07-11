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
            EventLabel = $"{dto.StartedAt:d} - {dto.EndedAt:d}",
            EloRatingFormatted = dto.EloRating.ToString("N0"),
            EloDelta = dto.EloDelta,
            VictoryPointsFormatted = dto.VictoryPoints.ToString("N0"),
            Tier = tier,
        };
    }
}
