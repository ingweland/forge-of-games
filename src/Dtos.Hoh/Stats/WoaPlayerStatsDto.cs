namespace Ingweland.Fog.Dtos.Hoh.Stats;

public class WoaPlayerStatsDto
{
    public int ContributionPoints { get; init; }
    public required DateTime EndedAt { get; init; }
    public int HealingDone { get; init; }
    public int LostAttacks { get; init; }
    public int RepairsStarted { get; init; }
    public required DateTime StartedAt { get; init; }
    public int VictoryPoints { get; init; }
    public int WonAttacks { get; init; }
    public int WonDefenses { get; init; }
}
