namespace Ingweland.Fog.Models.Hoh.Entities.Woa;

public class WoaDivision
{
    public required IReadOnlyCollection<int> AllianceIds { get; init; }
    public required int DivisionId { get; init; }
    public required IReadOnlyDictionary<int, double> EloDeltaByAllianceId { get; init; }
    public required IReadOnlyDictionary<int, double> EloRatingByAllianceId { get; init; }
    public required IReadOnlyDictionary<int, int> EnrolledMembersByAllianceId { get; init; }
    public required int EventId { get; init; }
    public required IReadOnlyDictionary<int, double> ExpectedVpShareByAllianceId { get; init; }
}
