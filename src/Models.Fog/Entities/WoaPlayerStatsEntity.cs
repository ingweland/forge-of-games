namespace Ingweland.Fog.Models.Fog.Entities;

public class WoaPlayerStatsEntity
{
    public required DateTime CollectedAt { get; set; }
    public long ContributionPoints { get; set; }
    public DateTime ContributionPointsGainedAt { get; set; }
    public int HealingDone { get; set; }
    public int Id { get; set; }
    public required int InGameEventId { get; set; }
    public int LostAttacks { get; set; }
    public int PlayerId { get; set; }
    public int RepairsStarted { get; set; }
    public float VictoryPoints { get; set; }
    public int WonAttacks { get; set; }
    public int WonDefenses { get; set; }
}
