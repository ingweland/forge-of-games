using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.Models;
using Ingweland.Fog.Application.Core.Helpers;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.WebApp.Client.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Ingweland.Fog.WebApp.Client.Services;

public class PageMetadataService(NavigationManager navigationManager, IStringLocalizer<FogResource> localizer)
    : IPageMetadataService
{
    public PageMetadata GetForCurrentPage()
    {
        var currentPageUri = new Uri(navigationManager.Uri);
        var currentPageAbsolutePath = currentPageUri.AbsolutePath;
        if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.BASE_COMMAND_CENTER_PATH))
        {
            if (currentPageAbsolutePath == FogUrlBuilder.PageRoutes.COMMAND_CENTER_EQUIPMENT_PATH)
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.CommandCenter_Equipment_PageTitle],
                    Description = localizer[FogResource.CommandCenter_Equipment_Meta_Description],
                    Keywords = localizer[FogResource.CommandCenter_Equipment_Meta_Keywords],
                    Title = localizer[FogResource.CommandCenter_Equipment_Title],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.COMMAND_CENTER_EQUIPMENT_PATH,
                    HelpPagePath = FogUrlBuilder.PageRoutes.HELP_EQUIPMENT_PATH,
                };
            }

            if (currentPageAbsolutePath == FogUrlBuilder.PageRoutes.MY_BATTLES_PATH)
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.CommandCenter_MyBattles_PageTitle],
                    Description = localizer[FogResource.CommandCenter_MyBattles_Meta_Description],
                    Keywords = localizer[FogResource.CommandCenter_MyBattles_Meta_Keywords],
                    Title = localizer[FogResource.CommandCenter_MyBattles_Title],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.MY_BATTLES_PATH,
                    HelpPagePath = FogUrlBuilder.PageRoutes.HELP_MY_BATTLES_PATH,
                };
            }

            if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes
                    .COMMAND_CENTER_EQUIPMENT_CONFIGURATOR_DASHBOARD_PATH))
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.CommandCenter_EquipmentConfigurator_PageTitle],
                    Description = localizer[FogResource.CommandCenter_EquipmentConfigurator_Meta_Description],
                    Keywords = localizer[FogResource.CommandCenter_EquipmentConfigurator_Meta_Keywords],
                    Title = localizer[FogResource.CommandCenter_EquipmentConfigurator_Title],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.COMMAND_CENTER_EQUIPMENT_CONFIGURATOR_DASHBOARD_PATH,
                    HelpPagePath = FogUrlBuilder.PageRoutes.BASE_HELP_PATH,
                };
            }

            return new PageMetadata
            {
                PageTitle = localizer[FogResource.CommandCenter_PageTitle],
                Description = localizer[FogResource.CommandCenter_Meta_Description],
                Keywords = localizer[FogResource.CommandCenter_Meta_Keywords],
                Title = localizer[FogResource.CommandCenter_Title],
                CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_COMMAND_CENTER_PATH,
                HelpPagePath = FogUrlBuilder.PageRoutes.HELP_COMMAND_CENTER_PATH,
            };
        }
        
        if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.ATLANTIS))
        {
            return new PageMetadata
            {
                PageTitle = localizer[FogResource.Atlantis_PageTitle],
                Description = localizer[FogResource.Atlantis_Meta_Description],
                Keywords = localizer[FogResource.Atlantis_Meta_Keywords],
                Title = localizer[FogResource.Atlantis_Title],
                CurrentHomePath = FogUrlBuilder.PageRoutes.ATLANTIS,
                HelpPagePath = FogUrlBuilder.PageRoutes.BASE_HELP_PATH,
            };
        }

        if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.BATTLE_LOG_PATH))
        {
            return new PageMetadata
            {
                PageTitle = localizer[FogResource.StatsHub_BattleLog_PageTitle],
                Description = localizer[FogResource.StatsHub_BattleLog_Meta_Description],
                Keywords = localizer[FogResource.StatsHub_BattleLog_Meta_Keywords],
                Title = localizer[FogResource.StatsHub_BattleLog_Title],
                CurrentHomePath = FogUrlBuilder.PageRoutes.BATTLE_LOG_PATH,
                HelpPagePath = FogUrlBuilder.PageRoutes.HELP_BATTLE_LOG_PATH,
            };
        }

        if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.BASE_STATS_HUB_PATH))
        {
            if (currentPageAbsolutePath.Contains("players") && currentPageAbsolutePath.EndsWith("profile"))
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.StatsHub_PlayerProfile_PageTitle],
                    Description = localizer[FogResource.StatsHub_Meta_Description],
                    Keywords = localizer[FogResource.StatsHub_Meta_Keywords],
                    Title = localizer[FogResource.StatsHub_PlayerProfile_Title],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_STATS_HUB_PATH,
                    HelpPagePath = FogUrlBuilder.PageRoutes.HELP_PLAYER_PROFILE_PATH,
                };
            }

            if (!currentPageAbsolutePath.Contains("worlds") && currentPageAbsolutePath.Contains("alliances"))
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.StatsHub_AllianceProfile_PageTitle],
                    Description = localizer[FogResource.StatsHub_Meta_Description],
                    Keywords = localizer[FogResource.StatsHub_Meta_Keywords],
                    Title = localizer[FogResource.StatsHub_AllianceProfile_Title],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_STATS_HUB_PATH,
                    HelpPagePath = FogUrlBuilder.PageRoutes.HELP_ALLIANCE_PROFILE_PATH,
                };
            }

            return new PageMetadata
            {
                PageTitle = localizer[FogResource.StatsHub_PageTitle],
                Description = localizer[FogResource.StatsHub_Meta_Description],
                Keywords = localizer[FogResource.StatsHub_Meta_Keywords],
                Title = localizer[FogResource.StatsHub_Title],
                CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_STATS_HUB_PATH,
                HelpPagePath = FogUrlBuilder.PageRoutes.HELP_STATS_HUB_PATH,
            };
        }

        // buildings
        if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.BASE_BUILDINGS_PATH))
        {
            if (currentPageAbsolutePath == FogUrlBuilder.PageRoutes.BASE_BUILDINGS_PATH)
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.Buildings_PageTitle],
                    Description = localizer[FogResource.Buildings_Meta_Description],
                    Keywords = localizer[FogResource.Buildings_Meta_Keywords],
                    Title = localizer[FogResource.Navigation_Buildings],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_BUILDINGS_PATH,
                };
            }

            return new PageMetadata
            {
                PageTitle = localizer[FogResource.Building_PageTitle],
                Description = localizer[FogResource.Building_Meta_Description],
                Keywords = localizer[FogResource.Building_Meta_Keywords],
                Title = localizer[FogResource.Navigation_Buildings],
                CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_BUILDINGS_PATH,
            };
        }

        // campaign
        if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.BASE_CAMPAIGN_PATH))
        {
            if (currentPageAbsolutePath == FogUrlBuilder.PageRoutes.BASE_CAMPAIGN_PATH)
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.Campaign_PageTitle],
                    Description = localizer[FogResource.Campaign_Meta_Description],
                    Keywords = localizer[FogResource.Campaign_Meta_Keywords],
                    Title = localizer[FogResource.Navigation_Campaign],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_CAMPAIGN_PATH,
                };
            }

            return new PageMetadata
            {
                PageTitle = localizer[FogResource.CampaignRegion_PageTitle],
                Description = localizer[FogResource.CampaignRegion_Meta_Description],
                Keywords = localizer[FogResource.CampaignRegion_Meta_Keywords],
                Title = localizer[FogResource.Navigation_Campaign],
                CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_CAMPAIGN_PATH,
            };
        }

        // city guides
        if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.ALLIED_CULTURE_CITY_GUIDES))
        {
            return new PageMetadata
            {
                PageTitle = localizer[FogResource.AlliedCultureCityGuides_PageTitle],
                Description = localizer[FogResource.AlliedCultureCityGuides_Meta_Description],
                Keywords = localizer[FogResource.AlliedCultureCityGuides_Meta_Keywords],
                Title = localizer[FogResource.AlliedCultureCityGuides_Title],
                CurrentHomePath = FogUrlBuilder.PageRoutes.ALLIED_CULTURE_CITY_GUIDES,
                HelpPagePath = FogUrlBuilder.PageRoutes.HELP_CITY_STRATEGY_BUILDER_APP_PATH,
            };
        }

        // city planner
        if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.BASE_CITY_PLANNER_PATH))
        {
            if (currentPageAbsolutePath == FogUrlBuilder.PageRoutes.CITY_PLANNER_INSPIRATIONS_PATH)
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.CityPlanner_Inspirations_PageTitle],
                    Description = localizer[FogResource.CityPlanner_Inspirations_Meta_Description],
                    Keywords = localizer[FogResource.CityPlanner_Inspirations_Meta_Keywords],
                    Title = localizer[FogResource.CityPlanner_Inspirations_Title],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.CITY_PLANNER_INSPIRATIONS_PATH,
                    HelpPagePath = FogUrlBuilder.PageRoutes.HELP_CITY_PLANNER_PATH,
                };
            }

            if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.CITY_STRATEGY_VIEWER_PATH))
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.CityStrategyViewer_PageTitle],
                    Description = localizer[FogResource.CityStrategyViewer_Meta_Description],
                    Keywords = localizer[FogResource.CityStrategyViewer_Meta_Keywords],
                    Title = localizer[FogResource.CityStrategyViewer_Title],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.ALLIED_CULTURE_CITY_GUIDES,
                    HelpPagePath = FogUrlBuilder.PageRoutes.HELP_CITY_STRATEGY_BUILDER_APP_PATH,
                };
            }

            if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.CITY_STRATEGY_BUILDER_APP_PATH))
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.CityStrategyBuilder_PageTitle],
                    Description = localizer[FogResource.CityStrategyBuilder_Meta_Description],
                    Keywords = localizer[FogResource.CityStrategyBuilder_Meta_Keywords],
                    Title = localizer[FogResource.CityStrategyBuilder_Title],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.CITY_STRATEGIES_DASHBOARD_PATH,
                    HelpPagePath = FogUrlBuilder.PageRoutes.HELP_CITY_STRATEGY_BUILDER_APP_PATH,
                };
            }

            if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.CITY_STRATEGIES_DASHBOARD_PATH))
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.CityStrategiesDashboard_PageTitle],
                    Description = localizer[FogResource.CityStrategiesDashboard_Meta_Description],
                    Keywords = localizer[FogResource.CityStrategiesDashboard_Meta_Keywords],
                    Title = localizer[FogResource.CityStrategiesDashboard_Title],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.CITY_STRATEGIES_DASHBOARD_PATH,
                    HelpPagePath = FogUrlBuilder.PageRoutes.HELP_CITY_STRATEGY_BUILDER_APP_PATH,
                };
            }

            if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.CITY_VIEWER_PATH))
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.CityViewer_PageTitle],
                    Description = localizer[FogResource.CityViewer_Meta_Description],
                    Keywords = localizer[FogResource.CityViewer_Meta_Keywords],
                    Title = localizer[FogResource.CityViewer_Title],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_CITY_PLANNER_PATH,
                    HelpPagePath = FogUrlBuilder.PageRoutes.HELP_CITY_PLANNER_PATH,
                };
            }

            if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.CITY_PLANNER_APP_PATH))
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.CityPlanner_PageTitle],
                    Description = localizer[FogResource.CityPlanner_Meta_Description],
                    Keywords = localizer[FogResource.CityPlanner_Meta_Keywords],
                    Title = localizer[FogResource.CityPlanner_Title],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_CITY_PLANNER_PATH,
                    HelpPagePath = FogUrlBuilder.PageRoutes.HELP_CITY_PLANNER_APP_PATH,
                };
            }

            if (currentPageAbsolutePath == FogUrlBuilder.PageRoutes.CITY_PLANNER_DATA_CONVERTER_PATH)
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.CityPlanner_DataConverter_PageTitle],
                    Description = localizer[FogResource.CityPlanner_DataConverter_Meta_Description],
                    Keywords = localizer[FogResource.CityPlanner_DataConverter_Meta_Keywords],
                    Title = localizer[FogResource.CityPlanner_DataConverter_Title],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.CITY_PLANNER_DATA_CONVERTER_PATH,
                    HelpPagePath = FogUrlBuilder.PageRoutes.BASE_HELP_PATH,
                };
            }

            return new PageMetadata
            {
                PageTitle = localizer[FogResource.CityPlanner_PageTitle],
                Description = localizer[FogResource.CityPlanner_Meta_Description],
                Keywords = localizer[FogResource.CityPlanner_Meta_Keywords],
                Title = localizer[FogResource.CityPlanner_Title],
                CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_CITY_PLANNER_PATH,
                HelpPagePath = FogUrlBuilder.PageRoutes.HELP_CITY_PLANNER_PATH,
            };
        }

        // heroes
        if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.BASE_HEROES_PATH))
        {
            if (currentPageAbsolutePath == FogUrlBuilder.PageRoutes.BASE_HEROES_PATH)
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.Heroes_PageTitle],
                    Description = localizer[FogResource.Heroes_Meta_Description],
                    Keywords = localizer[FogResource.Heroes_Meta_Keywords],
                    Title = localizer[FogResource.Hoh_Heroes],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_HEROES_PATH,
                    HelpPagePath = FogUrlBuilder.PageRoutes.HELP_HERO_PROFILE_PATH,
                };
            }

            return new PageMetadata
            {
                PageTitle = localizer[FogResource.Hero_PageTitle, FogResource.Hoh_Heroes],
                Description = localizer[FogResource.Hero_Meta_Description],
                Keywords = localizer[FogResource.Hero_Meta_Keywords],
                Title = localizer[FogResource.Hoh_Heroes],
                CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_HEROES_PATH,
                HelpPagePath = FogUrlBuilder.PageRoutes.HELP_HERO_PROFILE_PATH,
            };
        }

        if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.SUPPORT_US_PATH))
        {
            return new PageMetadata
            {
                PageTitle = localizer[FogResource.SupportUs_PageTitle],
                Description = localizer[FogResource.SupportUs_Meta_Description],
                Keywords = localizer[FogResource.SupportUs_Meta_Keywords],
                Title = localizer[FogResource.SupportUs_Title],
                CurrentHomePath = FogUrlBuilder.PageRoutes.SUPPORT_US_PATH,
            };
        }

        // tools
        if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.BASE_TOOLS_PATH))
        {
            if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.WONDER_COST_CALCULATOR_PATH))
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.WonderCostCalculator_PageTitle],
                    Description = localizer[FogResource.WonderCostCalculator_Meta_Description],
                    Keywords = localizer[FogResource.WonderCostCalculator_Meta_Keywords],
                    Title = localizer[FogResource.Navigation_Tools],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_TOOLS_PATH,
                    HelpPagePath = FogUrlBuilder.PageRoutes.HELP_TOOLS_PATH,
                };
            }

            if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.RESEARCH_CALCULATOR_PATH))
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.ResearchCalculator_PageTitle],
                    Description = localizer[FogResource.ResearchCalculator_Meta_Description],
                    Keywords = localizer[FogResource.ResearchCalculator_Meta_Keywords],
                    Title = localizer[FogResource.Navigation_Tools],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_TOOLS_PATH,
                    HelpPagePath = FogUrlBuilder.PageRoutes.HELP_TOOLS_PATH,
                };
            }

            if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.BUILDING_COST_CALCULATOR_PATH))
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.BuildingCostCalculator_PageTitle],
                    Description = localizer[FogResource.BuildingCostCalculator_Meta_Description],
                    Keywords = localizer[FogResource.BuildingCostCalculator_Meta_Keywords],
                    Title = localizer[FogResource.Navigation_Tools],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_TOOLS_PATH,
                    HelpPagePath = FogUrlBuilder.PageRoutes.HELP_TOOLS_PATH,
                };
            }

            return new PageMetadata
            {
                PageTitle = localizer[FogResource.Tools_PageTitle],
                Description = localizer[FogResource.Tools_Meta_Description],
                Keywords = localizer[FogResource.Tools_Meta_Keywords],
                Title = localizer[FogResource.Navigation_Tools],
                CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_TOOLS_PATH,
                HelpPagePath = FogUrlBuilder.PageRoutes.HELP_TOOLS_PATH,
            };
        }

        // treasure hunt
        if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.BASE_TREASURE_HUNT_PATH))
        {
            if (currentPageAbsolutePath == FogUrlBuilder.PageRoutes.BASE_TREASURE_HUNT_PATH)
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.TreasureHunt_PageTitle],
                    Description = localizer[FogResource.TreasureHunt_Meta_Description],
                    Keywords = localizer[FogResource.TreasureHunt_Meta_Keywords],
                    Title = localizer[FogResource.Navigation_TreasureHunt],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_TREASURE_HUNT_PATH,
                };
            }

            return new PageMetadata
            {
                PageTitle = localizer[FogResource.TreasureHuntStage_PageTitle],
                Description = localizer[FogResource.TreasureHuntStage_Meta_Description],
                Keywords = localizer[FogResource.TreasureHuntStage_Meta_Keywords],
                Title = localizer[FogResource.Navigation_TreasureHunt],
                CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_TREASURE_HUNT_PATH,
            };
        }

        if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.BASE_WONDERS_PATH))
        {
            if (currentPageAbsolutePath == FogUrlBuilder.PageRoutes.BASE_WONDERS_PATH)
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.Wonders_PageTitle],
                    Description = localizer[FogResource.Wonders_Meta_Description],
                    Keywords = localizer[FogResource.Wonders_Meta_Keywords],
                    Title = localizer[FogResource.Navigation_Wonders],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_WONDERS_PATH,
                };
            }

            return new PageMetadata
            {
                PageTitle = localizer[FogResource.Wonder_PageTitle],
                Description = localizer[FogResource.Wonder_Meta_Description],
                Keywords = localizer[FogResource.Wonder_Meta_Keywords],
                Title = localizer[FogResource.Navigation_Wonders],
                CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_WONDERS_PATH,
            };
        }

        if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.BASE_ABOUT_PATH))
        {
            return new PageMetadata
            {
                PageTitle = localizer[FogResource.About_PageTitle],
                Description = localizer[FogResource.About_Meta_Description],
                Keywords = localizer[FogResource.About_Meta_Keywords],
                Title = localizer[FogResource.Navigation_About],
                CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_ABOUT_PATH,
            };
        }

        // help
        if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.BASE_HELP_PATH))
        {
            if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.HELP_COMMAND_CENTER_PATH))
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.Help_CommandCenter_PageTitle],
                    Description = localizer[FogResource.Help_CommandCenter_Meta_Description],
                    Keywords = localizer[FogResource.Help_CommandCenter_Meta_Keywords],
                    Title = localizer[FogResource.Navigation_Help],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_HELP_PATH,
                };
            }

            if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.HELP_CITY_PLANNER_SNAPSHOTS_PATH))
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.Help_CityPlannerSnapshots_PageTitle],
                    Description = localizer[FogResource.Help_CityPlannerSnapshots_Meta_Description],
                    Keywords = localizer[FogResource.Help_CityPlannerSnapshots_Meta_Keywords],
                    Title = localizer[FogResource.Navigation_Help],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_HELP_PATH,
                };
            }

            if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.HELP_IMPORTING_IN_GAME_DATA_PATH))
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.Help_ImportingInGameData_PageTitle],
                    Description = localizer[FogResource.Help_ImportingInGameData_Meta_Description],
                    Keywords = localizer[FogResource.Help_ImportingInGameData_Meta_Keywords],
                    Title = localizer[FogResource.Navigation_Help],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_HELP_PATH,
                };
            }

            if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.HELP_BROWSER_EXTENSION_PATH))
            {
                return new PageMetadata
                {
                    PageTitle = localizer[FogResource.Help_BrowserExtension_PageTitle],
                    Description = localizer[FogResource.Help_BrowserExtension_Meta_Description],
                    Keywords = localizer[FogResource.Help_BrowserExtension_Meta_Keywords],
                    Title = localizer[FogResource.Navigation_Help],
                    CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_HELP_PATH,
                };
            }

            return new PageMetadata
            {
                PageTitle = localizer[FogResource.Help_PageTitle],
                Description = localizer[FogResource.Help_Meta_Description],
                Keywords = localizer[FogResource.Help_Meta_Keywords],
                Title = localizer[FogResource.Navigation_Help],
                CurrentHomePath = FogUrlBuilder.PageRoutes.BASE_HELP_PATH,
            };
        }

        // Anubis' Awakening
        if (currentPageAbsolutePath.StartsWith(FogUrlBuilder.PageRoutes.BASE_BATTLE_EVENTS_PATH))
        {
            return new PageMetadata
            {
                PageTitle = localizer[FogResource.AnubisAwakening_PageTitle],
                Description = localizer[FogResource.AnubisAwakening_Meta_Description],
                Keywords = localizer[FogResource.AnubisAwakening_Meta_Keywords],
                Title = localizer[FogResource.Navigation_AnubisAwakening],
                CurrentHomePath = FogUrlBuilder.PageRoutes.BattleEventRegion(RegionId.AncientEgyptDungeon),
                HelpPagePath = FogUrlBuilder.PageRoutes.BASE_HELP_PATH,
            };
        }

        // default
        return new PageMetadata
        {
            PageTitle = localizer[FogResource.Home_PageTitle],
            Description = localizer[FogResource.Home_Meta_Description],
            Keywords = localizer[FogResource.Home_Meta_Keywords],
            Title = localizer[FogResource.BrandName],
            CurrentHomePath = "/",
        };
    }
}
