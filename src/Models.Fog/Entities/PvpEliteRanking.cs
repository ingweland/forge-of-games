using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Models.Fog.Entities;

public class PvpEliteRanking
{
    public required DateOnly CollectedAt { get; set; }
    public int Id { get; set; }
    public int PlayerId { get; set; }

    public required EliteArenaTier Tier { get; set; }
}
