namespace Ingweland.Fog.Models.Fog.Entities;

public class AllianceWoaRanking
{
    public int AllianceId { get; set; }
    public required DateTime CollectedAt { get; set; }

    public required int DivisionId { get; set; }
    public int EloDelta { get; set; }
    public required int EloRating { get; set; }
    public required double ExpectedVictoryPointsShare { get; set; }
    public int Id { get; set; }
    public required int InGameEventId { get; set; }
    public required int VictoryPoints { get; set; }
}
