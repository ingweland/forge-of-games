using Ingweland.Fog.Application.Client.Web.Analytics;
using Ingweland.Fog.Application.Client.Web.Analytics.Interfaces;
using Ingweland.Fog.Application.Client.Web.CityStrategyBuilder.Abstractions;
using Ingweland.Fog.Application.Client.Web.Models;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.Application.Core.CityPlanner;
using Ingweland.Fog.Application.Core.Constants;
using Ingweland.Fog.Application.Core.Helpers;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.WebApp.Client.Components.Elements.CityPlanner;
using Ingweland.Fog.WebApp.Client.Components.Pages.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Ingweland.Fog.WebApp.Client.Components.Pages;

public partial class CityStrategiesDashboardPage : FogPageBase
{
    private bool _isSmallScreen;
    private IReadOnlyCollection<HohCityBasicData> _myStrategies = [];

    [Inject]
    private ICityStrategyAnalyticsService AnalyticsService { get; set; }

    [Inject]
    private IBrowserViewportService BrowserViewportService { get; set; }

    [Inject]
    public CityStrategyNavigationState CityStrategyNavigationState { get; set; }

    [Inject]
    private ICityStrategyUiService CityStrategyUiService { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject]
    private IPersistenceService PersistenceService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (!OperatingSystem.IsBrowser())
        {
            return;
        }

        var size = await BrowserViewportService.GetCurrentBrowserWindowSizeAsync();
        _isSmallScreen = size.Width < FogConstants.CITY_PLANNER_REQUIRED_SCREEN_WIDTH;

        _myStrategies = await PersistenceService.GetCityStrategies();

        AnalyticsService.TrackEvent(AnalyticsEvents.OPEN_CITY_STRATEGIES_DASHBOARD);
    }

    private async Task OpenStrategy(string strategyId)
    {
        var parameters = new Dictionary<string, object>
        {
            {AnalyticsParams.CITY_STRATEGY_ID, strategyId},
            {AnalyticsParams.IS_REMOTE, false},
        };
        AnalyticsService.TrackEvent(AnalyticsEvents.VIEW_CITY_STRATEGY_INIT, parameters);

        var strategy = await PersistenceService.LoadCityStrategy(strategyId);
        if (strategy == null)
        {
            AnalyticsService.TrackEvent(AnalyticsEvents.VIEW_CITY_STRATEGY_ERROR, parameters);
            return;
        }

        CityStrategyNavigationState.Data = new CityStrategyNavigationState.CityStrategyNavigationStateData
        {
            Strategy = strategy,
        };

        NavigationManager.NavigateTo(FogUrlBuilder.PageRoutes.CITY_STRATEGY_VIEWER_PATH);
    }

    private async Task CreateStrategy()
    {
        var options = GetDefaultDialogOptions();
        var dialog = await DialogService.ShowAsync<CreateNewCityDialog>(null, options);
        var result = await dialog.Result;
        if (result == null || result.Canceled)
        {
            return;
        }

        if (result.Data is not NewCityRequest newCityRequest)
        {
            return;
        }

        var strategy = CityStrategyUiService.CreateCityStrategy(newCityRequest);
        await PersistenceService.SaveCityStrategy(strategy);

        AnalyticsService.TrackCityStrategyCreation(newCityRequest);

        CityStrategyNavigationState.Data = new CityStrategyNavigationState.CityStrategyNavigationStateData
        {
            Strategy = strategy,
        };

        NavigationManager.NavigateTo(FogUrlBuilder.PageRoutes.CITY_STRATEGY_BUILDER_APP_PATH);
    }

    private static DialogOptions GetDefaultDialogOptions(bool closeButton = false)
    {
        return new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            BackgroundClass = "dialog-blur-bg",
            CloseButton = closeButton,
        };
    }
}
