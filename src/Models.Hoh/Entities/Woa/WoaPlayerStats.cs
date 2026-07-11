namespace Ingweland.Fog.Models.Hoh.Entities.Woa;

public class WoaPlayerStats
{
    public required HohPlayer Player { get; set; }
    public float VictoryPoints { get; set; }
    public int WonAttacks { get; set; }
    public int WonDefenses { get; set; }
    public int RepairsStarted { get; set; }
    public long ContributionPoints { get; set; }
    public int EventId { get; set; }
    public DateTime ContributionPointsGainedAt { get; set; }
    public int HealingDone { get; set; }
    public int LostAttacks { get; set; }
}
