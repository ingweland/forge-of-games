using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Dtos.Hoh.Stats;

public class PvpEliteRankingDto
{
    public required DateOnly CollectedAt { get; set; }
    public required EliteArenaTier Tier { get; set; }
}
