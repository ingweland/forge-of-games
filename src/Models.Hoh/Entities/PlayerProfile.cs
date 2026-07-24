using Ingweland.Fog.Models.Hoh.Entities.Alliance;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Models.Hoh.Entities;

public class PlayerProfile
{
    public HohAlliance? Alliance { get; init; }
    public required HohPlayer Player { get; init; }
    public EliteArenaTier PvpEliteTier { get; set; } = EliteArenaTier.Undefined;
    public PvpTier PvpTier { get; set; } = PvpTier.Undefined;
    public required int Rank { get; init; }
    public required int RankingPoints { get; init; }
    public IReadOnlyCollection<ProfileSquad> Squads { get; set; } = [];
    public int? TreasureHuntDifficulty { get; set; }
}
