using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Dtos.Hoh;

public class CommunityCityStrategyDto
{
    public string? AgeId { get; init; }
    public required string Author { get; init; }
    public required CityId CityId { get; init; }
    public bool IsPremium { get; init; }
    public required string Name { get; init; }
    public required string SharedDataId { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public WonderId? WonderId { get; init; }
}
