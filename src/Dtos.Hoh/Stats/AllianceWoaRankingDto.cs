using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Dtos.Hoh.Stats;

public class AllianceWoaRankingDto
{
    public required int DivisionId { get; init; }
    public required int EloDelta { get; init; }
    public required int EloRating { get; init; }
    public required DateTime EndedAt { get; init; }
    public required int EventId { get; init; }
    public required DateTime StartedAt { get; init; }
    public required WoaTier Tier { get; init; }
    public required int VictoryPoints { get; init; }
}
