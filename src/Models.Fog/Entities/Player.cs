using System.Text.Json.Serialization;
using Ingweland.Fog.Models.Fog.Enums;

namespace Ingweland.Fog.Models.Fog.Entities;

public class Player
{
    private PlayerKey? _key;
    public required string Age { get; set; }
    public ICollection<PlayerAgeHistoryEntry> AgeHistory { get; set; } = new List<PlayerAgeHistoryEntry>();
    public ICollection<Alliance> AllianceHistory { get; set; } = new List<Alliance>();
    public AllianceMemberEntity? AllianceMembership { get; set; }
    public ICollection<PlayerAthRanking> AthRankings { get; set; } = new List<PlayerAthRanking>();

    public int AvatarId { get; set; }
    public ICollection<PlayerCitySnapshot> CitySnapshots { get; set; } = new List<PlayerCitySnapshot>();
    public ICollection<EventCityStrategy> EventCityStrategies { get; set; } = new List<EventCityStrategy>();

    public ICollection<EventCityWonderRanking> EventCityWonderRankings { get; set; } =
        new List<EventCityWonderRanking>();

    public ICollection<PlayerHeroEntity> Heroes { get; set; } = new List<PlayerHeroEntity>();

    public int Id { get; set; }
    public required int InGamePlayerId { get; set; }

    [JsonIgnore]
    public PlayerKey Key
    {
        get { return _key ??= new PlayerKey(WorldId, InGamePlayerId); }
    }

    public DateTime? LastSeenOnline { get; set; }

    public required string Name { get; set; }
    public ICollection<PlayerNameHistoryEntry> NameHistory { get; set; } = new List<PlayerNameHistoryEntry>();
    public DateOnly ProfileUpdatedAt { get; set; }
    public ICollection<PvpBattle> PvpLosses { get; set; } = new List<PvpBattle>();

    [Obsolete($"Use {nameof(PvpRankings2)} instead.")]
    public ICollection<PvpRanking> PvpRankings { get; set; } = new List<PvpRanking>();

    public ICollection<PvpRanking2> PvpRankings2 { get; set; } = new List<PvpRanking2>();

    public ICollection<PvpBattle> PvpWins { get; set; } = new List<PvpBattle>();
    public int? Rank { get; set; }
    public int? RankingPoints { get; set; }
    public ICollection<PlayerRanking> Rankings { get; set; } = new List<PlayerRanking>();

    public ICollection<ProfileSquadEntity> Squads { get; set; } = new List<ProfileSquadEntity>();
    public InGameEntityStatus Status { get; set; } = InGameEntityStatus.Active;
    public int? TreasureHuntDifficulty { get; set; }
    public DateOnly UpdatedAt { get; set; }
    public ICollection<WoaPlayerStatsEntity> WoaStats { get; set; } = new List<WoaPlayerStatsEntity>();
    public required string WorldId { get; set; }
}
