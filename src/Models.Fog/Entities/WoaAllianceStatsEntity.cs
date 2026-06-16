using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Models.Fog.Entities;

public class WoaAllianceStatsEntity
{
    public int AllianceId { get; set; }
    public required DateTime CollectedAt { get; set; }
    public int Id { get; set; }
    public required int InGameEventId { get; set; }
    public required TreasureHuntLeague League { get; set; }
    public required int Points { get; set; }
}
