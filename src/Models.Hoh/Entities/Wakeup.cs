using Ingweland.Fog.Models.Hoh.Entities.Alliance;
using Ingweland.Fog.Models.Hoh.Entities.Ranking;
using Ingweland.Fog.Models.Hoh.Entities.Woa;

namespace Ingweland.Fog.Models.Hoh.Entities;

public class Wakeup
{
    public HohAlliance? Alliance { get; set; }

    public IReadOnlyCollection<HohAllianceExtended> Alliances { get; init; }
    public AllianceWithMembers? AllianceWithMembers { get; init; }

    public required IReadOnlyCollection<HeroTreasureHuntAlliancePoints> AthAllianceRankings { get; init; } =
        Array.Empty<HeroTreasureHuntAlliancePoints>();

    public required IReadOnlyCollection<HeroTreasureHuntPlayerPoints> AthPlayerRankings { get; init; } =
        Array.Empty<HeroTreasureHuntPlayerPoints>();

    public IReadOnlyCollection<Leaderboard> Leaderboards { get; init; } = [];
    public IReadOnlyCollection<WoaAlliance> WoaAlliances { get; init; } = [];

    public WoaDivision? WoaDivision { get; init; }
}
