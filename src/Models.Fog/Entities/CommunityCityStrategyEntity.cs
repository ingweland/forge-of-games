using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Models.Fog.Entities;

public class CommunityCityStrategyEntity
{
    public string? AgeId { get; set; }
    public required string Author { get; set; }
    public required CityId CityId { get; set; }
    public int Id { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsPremium { get; set; }
    public required string Name { get; set; }
    public required string SharedDataId { get; set; }

    public required DateTime UpdatedAt { get; set; }
    public WonderId? WonderId { get; set; }
}
