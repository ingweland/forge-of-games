using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.Providers.Interfaces;
using Ingweland.Fog.Application.Core.Helpers;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.WebApp.Client.Models;
using Ingweland.Fog.WebApp.Client.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Ingweland.Fog.WebApp.Client.Services;

public class MainMenuService(NavigationManager navigationManager, IAssetUrlProvider assetUrlProvider)
    : IMainMenuService
{
    public IReadOnlyCollection<NavMenuItem> GetMainMenuItems()
    {
        return new List<NavMenuItem>
        {
            new()
            {
                Href = "/",
                ResourceKey = FogResource.Navigation_Home,
                Icon = GetIconString(assetUrlProvider.GetHohIconUrl("icon_flat_home")),
                Match = NavLinkMatch.All,
            },
            new()
            {
                ResourceKey = FogResource.Navigation_CityLayouts,
                Icon = GetIconString(assetUrlProvider.GetIconUrl("dashboard_24dp_FBE0C6_FILL0_wght400_GRAD0_opsz24",
                    ".svg")),
                Children = new List<NavMenuItem>
                {
                    new()
                    {
                        Href = FogUrlBuilder.PageRoutes.BASE_CITY_PLANNER_PATH,
                        ResourceKey = FogResource.CityPlanner_Title,
                        Icon = GetIconString(
                            assetUrlProvider.GetIconUrl("architecture_30dp_FBE0C6_FILL0_wght700_GRAD200_opsz24",
                                ".svg")),
                        Match = NavLinkMatch.All,
                    },
                    new()
                    {
                        Href = FogUrlBuilder.PageRoutes.ALLIED_CULTURE_CITY_GUIDES,
                        ResourceKey = FogResource.Navigation_AlliedCultureCityGuides,
                        Icon = GetIconString(
                            assetUrlProvider.GetIconUrl("strategy_24dp_FBE0C6_FILL0_wght400_GRAD0_opsz24", ".svg")),
                        Match = NavLinkMatch.All,
                    },
                    new()
                    {
                        Href = FogUrlBuilder.PageRoutes.CITY_PLANNER_INSPIRATIONS_PATH,
                        ResourceKey = FogResource.Navigation_CityLayoutInspirations,
                        Icon = GetIconString(
                            assetUrlProvider.GetIconUrl("emoji_objects_24dp_FBE0C6_FILL0_wght400_GRAD0_opsz24",
                                ".svg")),
                        Match = NavLinkMatch.All,
                    },
                },
            },
            new()
            {
                Href = FogUrlBuilder.PageRoutes.BASE_HEROES_PATH,
                ResourceKey = FogResource.Hoh_Heroes,
                Icon = GetIconString(assetUrlProvider.GetHohIconUrl("icon_hud_heroes")),
            },
            new()
            {
                ResourceKey = FogResource.StatsHub_Title,
                Icon = GetIconString(assetUrlProvider.GetIconUrl("monitoring_24dp_FBE0C6_FILL0_wght400_GRAD0_opsz24",
                    ".svg")),
                Children = new List<NavMenuItem>
                {
                    new()
                    {
                        Href = FogUrlBuilder.PageRoutes.BASE_STATS_HUB_PATH,
                        ResourceKey = FogResource.StatsHub_Menu_Leaderboards,
                        Icon = GetIconString(assetUrlProvider.GetHohIconUrl("icon_flat_rank")),
                    },
                    new()
                    {
                        Href = FogUrlBuilder.PageRoutes.BATTLE_LOG_PATH,
                        ResourceKey = FogResource.StatsHub_Menu_BattleLog,
                        Icon = GetIconString(assetUrlProvider.GetHohIconUrl("icon_flat_pvp_results")),
                    },
                },
            },
            new()
            {
                ResourceKey = FogResource.CommandCenter_Title,
                Icon = GetIconString(assetUrlProvider.GetIconUrl("target_32dp_FBE0C6_FILL0_wght400_GRAD0_opsz40",
                    ".svg")),
                Children = new List<NavMenuItem>
                {
                    new()
                    {
                        Href = FogUrlBuilder.PageRoutes.COMMAND_CENTER_EQUIPMENT_CONFIGURATOR_DASHBOARD_PATH,
                        ResourceKey = FogResource.CommandCenter_Menu_EquipmentConfigurator,
                        Icon = GetIconString(assetUrlProvider.GetIconUrl("icon_equipment_configurator")),
                    },
                    new()
                    {
                        Href = FogUrlBuilder.PageRoutes.COMMAND_CENTER_PROFILES_PATH,
                        ResourceKey = FogResource.CommandCenter_Menu_Profiles,
                        Icon = GetIconString(
                            assetUrlProvider.GetIconUrl("target_32dp_FBE0C6_FILL0_wght400_GRAD0_opsz40", ".svg")),
                    },
                    new()
                    {
                        Href = FogUrlBuilder.PageRoutes.COMMAND_CENTER_EQUIPMENT_PATH,
                        ResourceKey = FogResource.CommandCenter_Menu_Equipment,
                        Icon = GetIconString(assetUrlProvider.GetHohIconUrl("icon_flat_equipment")),
                    },
                    new()
                    {
                        Href = FogUrlBuilder.PageRoutes.MY_BATTLES_PATH,
                        ResourceKey = FogResource.CommandCenter_Menu_MyBattles,
                        Icon = GetIconString(assetUrlProvider.GetHohIconUrl("icon_my_battles")),
                    },
                },
            },
            new()
            {
                Href = FogUrlBuilder.PageRoutes.BASE_TOOLS_PATH,
                ResourceKey = FogResource.Navigation_Tools,
                Icon = GetIconString(
                    assetUrlProvider.GetIconUrl("construction_28dp_FBE0C6_FILL0_wght700_GRAD200_opsz24")),
            },
            new()
            {
                Href = FogUrlBuilder.PageRoutes.BASE_CAMPAIGN_PATH,
                ResourceKey = FogResource.Navigation_Campaign,
                Icon = GetIconString(assetUrlProvider.GetHohIconUrl("icon_hud_map")),
            },
            new()
            {
                Href = FogUrlBuilder.PageRoutes.BASE_TREASURE_HUNT_PATH,
                ResourceKey = FogResource.Navigation_TreasureHunt,
                Icon = GetIconString(assetUrlProvider.GetHohIconUrl("icon_hud_battle")),
            },
            new()
            {
                Href = FogUrlBuilder.PageRoutes.BattleEventRegion(RegionId.AncientEgyptDungeon),
                ResourceKey = FogResource.Navigation_AnubisAwakening,
                Icon = GetIconString(assetUrlProvider.GetHohIconUrl("icon_flat_ancient_egypt")),
            },
            new()
            {
                Href = FogUrlBuilder.PageRoutes.BASE_BUILDINGS_PATH,
                ResourceKey = FogResource.Navigation_Buildings,
                Icon = GetIconString(assetUrlProvider.GetHohIconUrl("icon_hud_build")),
            },
            new()
            {
                Href = FogUrlBuilder.PageRoutes.BASE_WONDERS_PATH,
                ResourceKey = FogResource.Navigation_Wonders,
                Icon = GetIconString(assetUrlProvider.GetHohIconUrl("icon_flat_allied_culture")),
            },
            new()
            {
                Href = FogUrlBuilder.PageRoutes.SUPPORT_US_PATH,
                ResourceKey = FogResource.Navigation_SupportUs,
                Icon = GetIconString(
                    assetUrlProvider.GetIconUrl("volunteer_activism_36dp_FBE0C6_FILL0_wght400_GRAD0_opsz40",
                        ".svg")),
            },
            new()
            {
                Href = FogUrlBuilder.PageRoutes.BASE_ABOUT_PATH,
                ResourceKey = FogResource.Navigation_About,
                Icon = GetIconString(assetUrlProvider.GetIconUrl("info_24dp_FBE0C6_FILL0_wght400_GRAD0_opsz24",
                    ".svg")),
            },
            new()
            {
                Href = FogUrlBuilder.PageRoutes.BASE_HELP_PATH,
                ResourceKey = FogResource.Navigation_Help,
                Icon = GetIconString(assetUrlProvider.GetIconUrl("help_24dp_FBE0C6_FILL1_wght400_GRAD0_opsz24",
                    ".svg")),
            },
        };
    }

    public IReadOnlyCollection<NavMenuItem>? GetCurrentSectionMenuItems()
    {
        var currentPage = new Uri(navigationManager.Uri).AbsolutePath;
        if (currentPage.StartsWith(FogUrlBuilder.PageRoutes.BASE_COMMAND_CENTER_PATH))
        {
            return new List<NavMenuItem>
            {
                new()
                {
                    Href = "/command-center/profiles",
                    ResourceKey = FogResource.CommandCenter_Menu_Profiles,
                },
                new()
                {
                    ResourceKey = FogResource.CommandCenter_Menu_Playgrounds,
                    Children = new List<NavMenuItem>
                    {
                        new()
                        {
                            Href = "/command-center/playgrounds/heroes",
                            ResourceKey = FogResource.CommandCenter_Menu_Heroes,
                        },
                    },
                },
            };
        }

        return null;
    }

    private string GetIconString(string icon)
    {
        return $"<image width=\"100%\" height=\"100%\" xlink:href=\"{icon}\" preserveAspectRatio=\"xMidYMid meet\"/>";
    }
}
