using System.Text.Json.Serialization;

namespace Ingweland.Fog.Models.Fog.Entities;

public class PlayerHeroEntity
{
    private PlayerHeroKey? _key;
    public int Id { get; set; }

    [JsonIgnore]
    public PlayerHeroKey Key
    {
        get { return _key ??= new PlayerHeroKey(PlayerId, UnitId); }
    }

    public Player Player { get; set; } = null!;
    public int PlayerId { get; set; }

    public required string UnitId { get; set; }
}
