using Ingweland.Fog.Application.Client.Web.StatsHub.Abstractions;
using Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;
using Ingweland.Fog.Dtos.Hoh.Stats;

namespace Ingweland.Fog.Application.Client.Web.StatsHub.Factories;

public class WoaPlayerStatsViewModelFactory : IWoaPlayerStatsViewModelFactory
{
    public WoaPlayerStatsViewModel Create(WoaPlayerStatsDto dto)
    {
        return new WoaPlayerStatsViewModel
        {
            EventLabel = $"{dto.StartedAt:dd/MM} - {dto.EndedAt:dd/MM}",
            ContributionPointsFormatted = dto.ContributionPoints.ToString("N0"),
            VictoryPointsFormatted = dto.VictoryPoints.ToString("N0"),
            HealingDone = dto.HealingDone,
            LostAttacks = dto.LostAttacks,
            WonAttacks = dto.WonAttacks,
            WonDefenses = dto.WonDefenses,
            RepairsStarted = dto.RepairsStarted,
        };
    }
}
