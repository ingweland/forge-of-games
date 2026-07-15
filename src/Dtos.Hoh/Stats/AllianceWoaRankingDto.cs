using System.Text.Json.Serialization;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Dtos.Hoh.Stats;

public class AllianceWoaRankingDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required int DivisionId { get; init; }

    public int EloDelta { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required int EloRating { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required DateTime EndedAt { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required int EventId { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required DateTime StartedAt { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required WoaTier Tier { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required int VictoryPoints { get; init; }
}
