namespace Ingweland.Fog.Models.Fog.Entities;

public class WoaPlayerStatsEntity
{
    public int ContributionPoints { get; set; }
    public DateTime ContributionPointsGainedAt { get; set; }
    public int HealingDone { get; set; }
    public int Id { get; set; }
    public required int InGameEventId { get; set; }
    public int LostAttacks { get; set; }
    public int PlayerId { get; set; }
    public int RepairsStarted { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public int VictoryPoints { get; set; }
    public int WonAttacks { get; set; }
    public int WonDefenses { get; set; }
}
