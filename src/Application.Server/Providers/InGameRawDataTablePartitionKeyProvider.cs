using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Server.Providers;

public class InGameRawDataTablePartitionKeyProvider
{
    public string AllianceRankings(string worldId, DateOnly date, AllianceRankingType rankingType)
    {
        return WithWorldAndDate("alliance-rankings", worldId, date) + $"_{rankingType}";
    }

    public string PlayerRankings(string worldId, DateOnly date, PlayerRankingType rankingType)
    {
        return WithWorldAndDate("player-rankings", worldId, date) + $"_{rankingType}";
    }

    public string PvpRankings(string worldId, DateOnly date)
    {
        return WithWorldAndDate("pvp-rankings", worldId, date);
    }

    public string PvpBattles(string worldId, DateOnly date)
    {
        return WithWorldAndDate("pvp-battles", worldId, date);
    }

    public string BattleStats(string worldId, DateOnly date)
    {
        return WithWorldAndDate("battle-stats", worldId, date);
    }

    public string BattleCompleteWave(string worldId, DateOnly date)
    {
        return WithWorldAndDate("battle-complete-wave", worldId, date);
    }

    public string BattleStart(string worldId, DateOnly date)
    {
        return WithWorldAndDate("battle-start", worldId, date);
    }

    public string AthAllianceRankings(string worldId, DateOnly date)
    {
        return WithWorldAndDate("ath-alliance-rankings", worldId, date);
    }

    public string Alliance(string worldId, DateOnly date)
    {
        return WithWorldAndDate("alliance", worldId, date);
    }

    public string Woa(string worldId, DateOnly date)
    {
        return WithWorldAndDate("woa-wakeup", worldId, date);
    }

    public string WoaPlayerStats(string worldId, DateOnly date)
    {
        return WithWorldAndDate("woa-player-stats", worldId, date);
    }

    public string HeroesWakeup(string worldId, DateOnly date)
    {
        return WithWorldAndDate("heroes-wakeup", worldId, date);
    }

    public string HeroesStartup(string worldId, DateOnly date)
    {
        return WithWorldAndDate("heroes-startup", worldId, date);
    }

    private string WithWorldAndDate(string src, string worldId, DateOnly date)
    {
        return $"{src}_{worldId}_{date.ToString("O")}";
    }

    public string OtherCity(string worldId, DateOnly date)
    {
        return WithWorldAndDate("other-city", worldId, date);
    }
}
