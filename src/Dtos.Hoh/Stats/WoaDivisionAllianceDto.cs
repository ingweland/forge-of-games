namespace Ingweland.Fog.Dtos.Hoh.Stats;

public class WoaDivisionAllianceDto
{
    public required AllianceDto Alliance { get; init; }
    public required AllianceWoaRankingDto Ranking { get; init; }
}
