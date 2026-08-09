using Ingweland.Fog.Dtos.Hoh;

namespace Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;

public class AllianceWoaRankingViewModel
{
    public string? ActualVictoryPointsShareFormatted { get; init; }
    public required int DivisionId { get; init; }
    public int EloDelta { get; set; }
    public required string EloRatingFormatted { get; set; }
    public required string EventLabel { get; init; }
    public string? ExpectedVictoryPointsShareFormatted { get; init; }
    public required WoaTierDto Tier { get; init; }
    public required string VictoryPointsFormatted { get; set; }
    public int VictoryPointsShareComparison { get; init; }
}
