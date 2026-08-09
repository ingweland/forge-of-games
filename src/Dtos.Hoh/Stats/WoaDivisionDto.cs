using System.Text.Json.Serialization;

namespace Ingweland.Fog.Dtos.Hoh.Stats;

public class WoaDivisionDto
{
    public required IReadOnlyCollection<WoaDivisionAllianceDto> Alliances { get; init; }

    public required int DivisionId { get; init; }

    public required DateTime EndedAt { get; init; }

    public required DateTime StartedAt { get; init; }
}