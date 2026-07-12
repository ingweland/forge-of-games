namespace Ingweland.Fog.Application.Client.Web.StatsHub.ViewModels;

public class WoaPlayerStatsViewModel
{
    public string ContributionPointsFormatted { get; init; }
    public required string EventLabel { get; init; }
    public int HealingDone { get; init; }
    public int LostAttacks { get; init; }
    public int RepairsStarted { get; init; }
    public string VictoryPointsFormatted { get; init; }
    public int WonAttacks { get; init; }
    public int WonDefenses { get; init; }
}
