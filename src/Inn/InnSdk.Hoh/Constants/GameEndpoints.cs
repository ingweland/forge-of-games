namespace Ingweland.Fog.InnSdk.Hoh.Constants;

public static class GameEndpoints
{
    public static readonly string WebLoginUrl = "https://{0}.heroesgame.com/api/login";
    public static readonly string AccountPlayUrl = "https://{0}0.heroesofhistorygame.com/core/api/account/play";
    public static readonly string BaseApiUrl = "https://{0}.heroesofhistorygame.com";
    
    public static readonly string AllianceMembersPath = "game/alliances/members";
    public static readonly string AllianceRankingPath = "game/ranking/alliance";
    public static readonly string AllianceSearchPath = "game/alliances/search"; 
    public static readonly string AlliancePath = "game/alliances/get"; 
    public static readonly string BatchPath = "game/batch"; 
    public static readonly string BattleStatsPath = "game/battle/hero/stats";
    public static readonly string EventRankingPath = "game/event-ranking/get";
    public static readonly string GameDesignPath = "game/gamedesign";
    public static readonly string LocaPath = "game/loca";
    public static readonly string PlayerProfilePath = "game/player/profile";
    public static readonly string SearchPlayerPath = "game/player/search";
    public static readonly string PlayerRankingPath = "game/ranking/player";
    public static readonly string PvpRankingPath = "game/pvp/get-ranking";
    public static readonly string StartupPath = "game/startup";
    public static readonly string VisitCityPath = "game/visit-city";
    public static readonly string WoaConquestLogPath = "game/woa/conquest-log";

    public static string CreateUrl(string serverId, string path)
    {
        var baseUrl = string.Format(BaseApiUrl, serverId);
        return $"{baseUrl}/{path}";
    }
}
