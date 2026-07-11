using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Core.Helpers;

public static class FogUrlBuilder
{
    private static string BuildPath(params string[] segments)
    {
        var cleanSegments = segments
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim('/'));

        return "/" + string.Join("/", cleanSegments);
    }

    public static class ApiRoutes
    {
        private const string BASE_STATS_PATH = "stats";
        private const string BASE_BATTLES_PATH = "battles";

        public const string WIKI_EXTRACT = "/wiki/extract";

        public const string PLAYERS_TEMPLATE = "/" + BASE_STATS_PATH + "/worlds/{worldId}/players";
        public const string PLAYER_TEMPLATE = "/" + BASE_STATS_PATH + "/players/{playerId:int}";
        public const string PLAYER_TEMPLATE_REFIT = "/" + BASE_STATS_PATH + "/players/{playerId}";
        public const string PLAYER_PROFILE_TEMPLATE = "/" + BASE_STATS_PATH + "/players/{playerId:int}/profile";
        public const string PLAYER_PROFILE_TEMPLATE_REFIT = "/" + BASE_STATS_PATH + "/players/{playerId}/profile";
        public const string PLAYER_CITY_TEMPLATE = "/" + BASE_STATS_PATH + "/players/{playerId:int}/city";
        public const string PLAYER_EVENT_CITY_TEMPLATE = "/" + BASE_STATS_PATH + "/players/{playerId:int}/eventCity";
        public const string PLAYER_CITY_TEMPLATE_REFIT = "/" + BASE_STATS_PATH + "/players/{playerId}/city";
        public const string PLAYER_EVENT_CITY_TEMPLATE_REFIT = "/" + BASE_STATS_PATH + "/players/{playerId}/eventCity";
        public const string PLAYER_BATTLES_TEMPLATE = "/" + BASE_STATS_PATH + "/players/{playerId:int}/battles";
        public const string PLAYER_BATTLES_TEMPLATE_REFIT = "/" + BASE_STATS_PATH + "/players/{playerId}/battles";
        public const string PLAYER_PVP_RANKINGS_TEMPLATE = PLAYER_TEMPLATE + "/pvpRankings";
        public const string PLAYER_PVP_RANKINGS_TEMPLATE_REFIT = PLAYER_TEMPLATE_REFIT + "/pvpRankings";
        public const string TOP_PLAYERS_TEMPLATE = PLAYERS_TEMPLATE + "/top";
        public const string PLAYER_RANKINGS_TEMPLATE = PLAYER_TEMPLATE + "/rankings";
        public const string PLAYER_RANKINGS_TEMPLATE_REFIT = PLAYER_TEMPLATE_REFIT + "/rankings";
        public const string PLAYER_PRODUCTION_CAPACITY_TEMPLATE = PLAYER_TEMPLATE + "/productionCapacity";
        public const string PLAYER_PRODUCTION_CAPACITY_TEMPLATE_REFIT = PLAYER_TEMPLATE_REFIT + "/productionCapacity";
        public const string PLAYER_WONDER_RANKINGS_TEMPLATE_REFIT = PLAYER_TEMPLATE_REFIT + "/wonderRankings";
        public const string PLAYER_WONDER_RANKINGS_TEMPLATE = PLAYER_TEMPLATE + "/wonderRankings";
        public const string PLAYER_ATH_RANKINGS_TEMPLATE = PLAYER_TEMPLATE + "/athRankings";
        public const string PLAYER_ATH_RANKINGS_TEMPLATE_REFIT = PLAYER_TEMPLATE_REFIT + "/athRankings";

        public const string WORLD_EVENT_CITY_TEMPLATE = "/" + BASE_STATS_PATH + "/worlds/{worldId}/eventCities";

        public const string ALLIANCES_TEMPLATE = "/" + BASE_STATS_PATH + "/worlds/{worldId}/alliances";
        public const string ALLIANCE_TEMPLATE = "/" + BASE_STATS_PATH + "/alliances/{allianceId:int}";
        public const string ALLIANCE_TEMPLATE_REFIT = "/" + BASE_STATS_PATH + "/alliances/{allianceId}";
        public const string ALLIANCE_RANKINGS_TEMPLATE = ALLIANCE_TEMPLATE + "/rankings";
        public const string ALLIANCE_RANKINGS_TEMPLATE_REFIT = ALLIANCE_TEMPLATE_REFIT + "/rankings";
        public const string ALLIANCE_ATH_RANKINGS_TEMPLATE = ALLIANCE_TEMPLATE + "/athRankings";
        public const string ALLIANCE_ATH_RANKINGS_TEMPLATE_REFIT = ALLIANCE_TEMPLATE_REFIT + "/athRankings";
        public const string ALLIANCES_ATH_RANKINGS_TEMPLATE = ALLIANCES_TEMPLATE + "/athRankings";
        public const string TOP_ALLIANCES_TEMPLATE = ALLIANCES_TEMPLATE + "/top";
        public const string ALLIANCE_WOA_RANKINGS_TEMPLATE = ALLIANCE_TEMPLATE + "/woaRankings";
        public const string ALLIANCE_WOA_RANKINGS_TEMPLATE_REFIT = ALLIANCE_TEMPLATE_REFIT + "/woaRankings";

        public const string BATTLE_LOG_SEARCH = "/battle-log/search";
        public const string BATTLE_STATS_TEMPLATE = "/" + BASE_BATTLES_PATH + "/stats/{battleStatsId:int}";
        public const string BATTLE_STATS_TEMPLATE_REFIT = "/" + BASE_BATTLES_PATH + "/stats/{battleStatsId}";
        public const string BATTLE_TEMPLATE = "/" + BASE_BATTLES_PATH + "/battles/{battleId:int}";
        public const string BATTLE_TEMPLATE_REFIT = "/" + BASE_BATTLES_PATH + "/battles/{battleId}";
        public const string UNIT_BATTLES_TEMPLATE = "/units/{unitId}/battles/{battleType}";

        public const string PLAYER_CITY_SNAPSHOTS_SEARCH = "/playerCitySnapshots/search";
        public const string PLAYER_CITY_SNAPSHOT_TEMPLATE = "/playerCitySnapshots/{snapshotId:int}";
        public const string PLAYER_CITY_SNAPSHOT_TEMPLATE_REFIT = "/playerCitySnapshots/{snapshotId}";

        public const string COMMON_AGES = "/common/ages";
        public const string COMMON_RESOURCES = "/common/resources";
        public const string COMMON_CITIES = "/common/cities";
        public const string COMMON_PVP_TIERS = "/common/pvpTiers";
        public const string COMMON_TREASURE_HUNT_LEAGUES = "/common/athLeagues";

        public const string BASE_HEROES_PATH = "/" + BASE_STATS_PATH + "/heroes";
        public const string TOP_HEROES_PATH = BASE_HEROES_PATH + "/top";
        public const string HERO_ABILITY_FEATURES = BASE_HEROES_PATH + "/abilityFeatures";

        public const string RELICS_INSIGHTS_TEMPLATE = "/relics/insights/{unitId}";

        public const string USER_BATTLE_SEARCH = "/userBattleSearch";

        public const string COMMAND_CENTER_SHARED_SUBMISSION_ID = "/commandCenter/sharedSubmissionId";
        public const string EQUIPMENT_INSIGHTS_TEMPLATE = "/equipment/insights/{unitId}";
        public const string IN_GAME_EVENTS_TEMPLATE = "/inGameEvents/{worldId}/{eventDefinitionId}";
        public const string CURRENT_IN_GAME_EVENT_TEMPLATE = "/currentInGameEvent/{worldId}/{eventDefinitionId}";
        public const string ANNUAL_BUDGET_TEMPLATE = "/annualBudget/{year:int}";
        public const string ANNUAL_BUDGET_TEMPLATE_REFIT = "/annualBudget/{year}";
        public const string CREATE_SHARE = "/shares";
        public const string GET_SHARED_RESOURCE_TEMPLATE = "/shares/{shareId}";
        public const string GET_COMMUNITY_CITY_STRATEGIES = "/communityCityStrategies";
        public const string GET_COMMUNITY_CITY_GUIDES = "/communityCityGuides";
        public const string GET_COMMUNITY_CITY_GUIDE_TEMPLATE = GET_COMMUNITY_CITY_GUIDES + "/{id:int}";
        public const string GET_COMMUNITY_CITY_GUIDE_TEMPLATE_REFIT = GET_COMMUNITY_CITY_GUIDES + "/{id}";
        public const string UPLOAD_SHARED_IMAGE = "/uploadSharedImage";

        public const string HOH_CORE_DATA = "/coreData";
        public const string HOH_LOCALIZATION_DATA = HOH_CORE_DATA + "/localization";
        public const string HOH_CORE_DATE_VERSION = HOH_CORE_DATA + "/version";
    }

    public static class PageRoutes
    {
        public const string BASE_ABOUT_PATH = "/about";
        public const string BASE_BUILDINGS_PATH = "/buildings";
        public const string BASE_CAMPAIGN_PATH = "/campaign";
        public const string BASE_CITY_PLANNER_PATH = "/city-planner";
        public const string CITY_PLANNER_APP_PATH = BASE_CITY_PLANNER_PATH + "/app";
        public const string CITY_PLANNER_INSPIRATIONS_PATH = BASE_CITY_PLANNER_PATH + "/inspirations";
        public const string CITY_STRATEGIES_DASHBOARD_PATH = BASE_CITY_PLANNER_PATH + "/strategies";
        public const string CITY_STRATEGY_BUILDER_APP_PATH = CITY_STRATEGIES_DASHBOARD_PATH + "/app";
        public const string CITY_STRATEGY_VIEWER_PATH = CITY_STRATEGIES_DASHBOARD_PATH + "/viewer";
        public const string BASE_CITY_GUIDES_PATH = CITY_STRATEGIES_DASHBOARD_PATH + "/guides";
        public const string CITY_GUIDE_TEMPLATE = BASE_CITY_GUIDES_PATH + "/{guideId:int}";
        public const string CITY_VIEWER_PATH = BASE_CITY_PLANNER_PATH + "/viewer";
        public const string BASE_HEROES_PATH = "/heroes";
        public const string BASE_STATS_HUB_PATH = "/stats-hub";
        public const string BASE_TOOLS_PATH = "/tools";
        public const string BASE_TREASURE_HUNT_PATH = "/treasure-hunt";
        public const string BASE_WONDERS_PATH = "/wonders";
        public const string BASE_HELP_PATH = "/help";
        public const string HELP_COMMAND_CENTER_PATH = BASE_HELP_PATH + "/command-center";
        public const string HELP_HERO_PROFILE_PATH = HELP_COMMAND_CENTER_PATH + "/hero-profile";
        public const string HELP_EQUIPMENT_PATH = BASE_HELP_PATH + "/equipment";
        public const string HELP_CITY_PLANNER_SNAPSHOTS_PATH = BASE_HELP_PATH + "/city-planner-snapshots";
        public const string HELP_CITY_PLANNER_PATH = BASE_HELP_PATH + "/city-planner";
        public const string HELP_CITY_PLANNER_APP_PATH = BASE_HELP_PATH + "/city-planner-app";
        public const string HELP_CITY_STRATEGY_BUILDER_APP_PATH = BASE_HELP_PATH + "/city-strategy-builder-app";
        public const string HELP_BROWSER_EXTENSION_PATH = BASE_HELP_PATH + "/browser-extension";
        public const string HELP_IMPORTING_IN_GAME_DATA_PATH = BASE_HELP_PATH + "/importing-hoh-data";
        public const string HELP_STATS_HUB_PATH = BASE_HELP_PATH + "/stats-hub";
        public const string HELP_BATTLE_LOG_PATH = BASE_HELP_PATH + "/battle-log";
        public const string HELP_PLAYER_PROFILE_PATH = BASE_HELP_PATH + "/player-profile";
        public const string HELP_ALLIANCE_PROFILE_PATH = BASE_HELP_PATH + "/alliance-profile";
        public const string HELP_TOOLS_PATH = BASE_HELP_PATH + "/tools";
        public const string HELP_SUBMISSION_ID_PATH = BASE_HELP_PATH + "/submission-id";
        public const string HELP_MY_BATTLES_PATH = BASE_HELP_PATH + "/my-battles";
        public const string HERO_TEMPLATE = BASE_HEROES_PATH + "/{heroId}";
        public const string CAMPAIGN_REGION_TEMPLATE = BASE_CAMPAIGN_PATH + "/region/{regionId}";
        public const string BASE_BATTLE_EVENTS_PATH = "/battle-events";
        public const string BATTLE_EVENT_REGION_TEMPLATE = BASE_BATTLE_EVENTS_PATH + "/regions/{regionId}";
        public const string BUILDING_TEMPLATE = BASE_BUILDINGS_PATH + "/{cityId}/{buildingGroup}";
        public const string RESEARCH_CALCULATOR_PATH = BASE_TOOLS_PATH + "/research-calculator";
        public const string WONDER_COST_CALCULATOR_PATH = BASE_TOOLS_PATH + "/wonder-cost-calculator";
        public const string BUILDING_COST_CALCULATOR_PATH = BASE_TOOLS_PATH + "/building-cost-calculator";
        public const string WONDER_TEMPLATE = BASE_WONDERS_PATH + "/{wonderId}";

        public const string TREASURE_HUNT_STAGE_TEMPLATE =
            BASE_TREASURE_HUNT_PATH + "/{difficulty:int}/{stageIndex:int}";

        public const string SUPPORT_US_PATH = "/support-us";
        public const string WORLD_PLAYERS_TEMPLATE = BASE_STATS_HUB_PATH + "/worlds/{worldId}/players";
        public const string WORLD_ALLIANCES_TEMPLATE = BASE_STATS_HUB_PATH + "/worlds/{worldId}/alliances";
        public const string WORLD_ALLIANCES_ATH_TEMPLATE = WORLD_ALLIANCES_TEMPLATE + "/ath";
        public const string PLAYER_PROFILE_TEMPLATE = BASE_STATS_HUB_PATH + "/players/{playerId:int}/profile";
        public const string PLAYER_BATTLES_TEMPLATE = BASE_STATS_HUB_PATH + "/players/{playerId:int}/battles";
        public const string ALLIANCE_TEMPLATE = BASE_STATS_HUB_PATH + "/alliances/{allianceId:int}";
        public const string TOP_HEROES_PATH = BASE_STATS_HUB_PATH + "/top-heroes";
        public const string WORLD_EVENT_CITY_TEMPLATE = BASE_STATS_HUB_PATH + "/worlds/{worldId}/eventCities";

        public const string FOG_GITHUB_URL = "https://github.com/IngweLand/forge-of-games";
        public const string HOH_HELPER_GITHUB_URL = "https://github.com/IngweLand/hoh-helper";

        public const string HOH_HELPER_CHROME_WEBSTORE_URL =
            "https://chromewebstore.google.com/detail/hoh-helper-forge-of-games/almhmnmbpfaonomgaconnmcogadnjndf";

        public const string HOH_HELPER_RELEASES_GITHUB_URL = HOH_HELPER_GITHUB_URL + "/releases";
        public const string FOG_DISCORD_URL = "https://discord.gg/4vFeeh7CZn";
        public const string CITIES_STATS_PATH = "/cities-stats";
        public const string BATTLE_LOG_PATH = "/battle-log";
        public const string BATTLE_TEMPLATE = "/battle-log/battles/{battleId:int}";

        public const string PATREON_URL = "https://www.patreon.com/forgeofgames/about";
        public const string PAYPAL_DONATION_URL = "https://www.paypal.com/donate/?hosted_button_id=AT6HXM5BRZXGC";

        public const string PAYPAL_DONATION_FOG_PLUS_STRATEGIES_URL =
            "https://www.paypal.com/donate/?hosted_button_id=KTXFFCPCFDNWU";

        public const string PAYPAL_ME_URL = "https://paypal.me/ingwelandat";
        public const string MAINTENANCE_PAGE = "/maintenance.html";

        public const string BASE_COMMAND_CENTER_PATH = "/command-center";
        public const string COMMAND_CENTER_PROFILES_PATH = BASE_COMMAND_CENTER_PATH + "/profiles";
        public const string COMMAND_CENTER_HERO_PLAYGROUNDS_PATH = BASE_COMMAND_CENTER_PATH + "/playgrounds/heroes";
        public const string COMMAND_CENTER_EQUIPMENT_PATH = BASE_COMMAND_CENTER_PATH + "/equipment";

        public const string COMMAND_CENTER_EQUIPMENT_CONFIGURATOR_TEMPLATE =
            BASE_COMMAND_CENTER_PATH + "/equipment-configurator/app/{profileId}";

        public const string COMMAND_CENTER_EQUIPMENT_CONFIGURATOR_DASHBOARD_PATH =
            BASE_COMMAND_CENTER_PATH + "/equipment-configurator";

        public const string MY_BATTLES_PATH = BASE_COMMAND_CENTER_PATH + "/my-battles";

        public const string GET_SHARED_STRATEGY_TEMPLATE = CITY_STRATEGIES_DASHBOARD_PATH + "/shares/{shareId}";
        public const string GET_SHARED_CITY_TEMPLATE = BASE_CITY_PLANNER_PATH + "/shares/{shareId}";

        public const string GET_SHARED_EQUIPMENT_PROFILE_TEMPLATE =
            COMMAND_CENTER_EQUIPMENT_CONFIGURATOR_DASHBOARD_PATH + "/shares/{shareId}";

        public const string CITY_PLANNER_DATA_CONVERTER_PATH = "/city-planner-data-converter";
        public const string CITY_GUIDE_CONVERTER_PATH = "/city-guide-converter";

        public const string ALLIED_CULTURE_CITY_GUIDES = "/allied-culture-city-guides";
        public const string ATLANTIS = "/battle-for-atlantis";

        public static string Player(int id)
        {
            return PLAYER_PROFILE_TEMPLATE.Replace("{playerId:int}", id.ToString());
        }

        public static string Hero(string heroId)
        {
            return HERO_TEMPLATE.Replace("{heroId}", heroId);
        }

        public static string Alliance(int id)
        {
            return ALLIANCE_TEMPLATE.Replace("{allianceId:int}", id.ToString());
        }

        public static string SearchAlliance(string worldId, string name)
        {
            return $"{WORLD_ALLIANCES_TEMPLATE.Replace("{worldId}", worldId)}?name={Uri.EscapeDataString(name)}";
        }

        public static string TreasureHuntStage(int difficulty, int stageIndex)
        {
            return BuildPath(BASE_TREASURE_HUNT_PATH, difficulty.ToString(), stageIndex.ToString());
        }

        public static string WorldPlayers(string worldId)
        {
            if (string.IsNullOrWhiteSpace(worldId))
            {
                throw new ArgumentNullException(nameof(worldId));
            }

            return WORLD_PLAYERS_TEMPLATE.Replace("{worldId}", worldId);
        }

        public static string WorldAlliances(string worldId)
        {
            if (string.IsNullOrWhiteSpace(worldId))
            {
                throw new ArgumentNullException(nameof(worldId));
            }

            return WORLD_ALLIANCES_TEMPLATE.Replace("{worldId}", worldId);
        }

        public static string WorldAlliancesAth(string worldId)
        {
            if (string.IsNullOrWhiteSpace(worldId))
            {
                throw new ArgumentNullException(nameof(worldId));
            }

            return WORLD_ALLIANCES_ATH_TEMPLATE.Replace("{worldId}", worldId);
        }

        public static string PlayerBattles(int playerId)
        {
            return PLAYER_BATTLES_TEMPLATE.Replace("{playerId:int}", playerId.ToString());
        }

        public static string Battle(int battleId)
        {
            return BATTLE_TEMPLATE.Replace("{battleId:int}", battleId.ToString());
        }

        public static string EquipmentProfile(string profileId)
        {
            return COMMAND_CENTER_EQUIPMENT_CONFIGURATOR_TEMPLATE.Replace("{profileId}", profileId);
        }

        public static string CommunityCityGuide(int guideId)
        {
            return CITY_GUIDE_TEMPLATE.Replace("{guideId:int}", guideId.ToString());
        }

        public static string WorldEventCity(string worldId)
        {
            if (string.IsNullOrWhiteSpace(worldId))
            {
                throw new ArgumentNullException(nameof(worldId));
            }

            return WORLD_EVENT_CITY_TEMPLATE.Replace("{worldId}", worldId);
        }

        public static string BattleEventRegion(RegionId regionId)
        {
            return BATTLE_EVENT_REGION_TEMPLATE.Replace("{regionId}", regionId.ToString());
        }
    }
}
