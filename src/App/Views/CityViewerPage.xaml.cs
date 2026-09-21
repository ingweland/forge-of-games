using Ingweland.Fog.Application.Client.Web.CityPlanner.Abstractions;
using Ingweland.Fog.Application.Core.Constants;
using Ingweland.Fog.Models.Fog.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.App.Views;

/// <summary>
///     Read-only city map: the MAUI counterpart of WebApp.Client's CityViewerComponent and
///     CityMobileViewerComponent. The map itself, the property cards beside it and the cost calculators under
///     them are <see cref="CityViewer.CityMapView" />, which a guide's layout timeline item shares; this page
///     initializes the city planner behind it and owns the buttons over it.
/// </summary>
public partial class CityViewerPage : ContentPage, IQueryAttributable
{
    public const string CITY_QUERY_KEY = "City";

    private readonly ICityPlanner _cityPlanner;
    private readonly ILogger<CityViewerPage> _logger;
    private readonly IServiceScope _scope;

    private bool _initializationStarted;
    private bool _isReleased;
    private bool? _isWide;
    private bool _mapIsReady;

    public CityViewerPage(IServiceScopeFactory serviceScopeFactory, ILogger<CityViewerPage> logger)
    {
        _logger = logger;

        // The city planner services are scoped, and the web gets a fresh set per page visit. MAUI would
        // resolve them from the window scope instead, sharing pan/zoom and selection across visits, so
        // each viewer owns its own scope.
        _scope = serviceScopeFactory.CreateScope();
        _cityPlanner = _scope.ServiceProvider.GetRequiredService<ICityPlanner>();

        InitializeComponent();

        Map.StateChanged += OnMapStateChanged;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (_initializationStarted || !query.TryGetValue(CITY_QUERY_KEY, out var value) || value is not HohCity city)
        {
            return;
        }

        _initializationStarted = true;
        Title = city.Name;
        _ = InitializeAsync(city);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        // Route pages are created per navigation, so once this one is popped it is never shown again.
        if (args.NavigationType is NavigationType.Pop or NavigationType.PopToRoot ||
            !Navigation.NavigationStack.Contains(this))
        {
            Release();
        }
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0)
        {
            return;
        }

        var isWide = width >= FogConstants.CITY_PLANNER_REQUIRED_SCREEN_WIDTH;
        if (isWide == _isWide)
        {
            return;
        }

        _isWide = isWide;
        Map.ApplyLayoutMode(isWide);
        // The side panel is always there on a wide window; on a narrow one the sheet is asked for.
        AnalyticsButton.IsVisible = !isWide;
        UpdateToolbar();
    }

    private async Task InitializeAsync(HohCity city)
    {
        try
        {
            await _cityPlanner.InitializeAsync(city);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not initialize city {CityId}", city.Id);
            if (!_isReleased)
            {
                LoadingIndicator.IsRunning = false;
                ErrorLabel.Text = e.Message;
                ErrorLabel.IsVisible = true;
                Map.ShowFailure();
            }

            return;
        }

        if (_isReleased)
        {
            return;
        }

        LoadingIndicator.IsRunning = false;
        _mapIsReady = true;

        Map.Initialize(_scope.ServiceProvider);
        Map.Refresh();
        UpdateToolbar();
    }

    private void OnMapStateChanged()
    {
        UpdateToolbar();
    }

    private void UpdateToolbar()
    {
        // These float over the map, and on a narrow window the panel covers it whole - they would sit on
        // top of the panel, the analytics button right over its close button. The close button is the way
        // back, so the toolbar stands down until the map is showing again.
        Toolbar.IsVisible = _mapIsReady && !Map.IsPanelOpen;

        if (Map.IsCityPropertiesToggled)
        {
            AnalyticsButton.BackgroundColor = ThemeResources.Color("FogPrimaryColor");
        }
        else
        {
            AnalyticsButton.ClearValue(BackgroundColorProperty);
        }
    }

    private void OnAnalyticsClicked(object? sender, EventArgs e)
    {
        Map.ToggleCityProperties();
    }

    private void OnZoomInClicked(object? sender, EventArgs e)
    {
        Map.ZoomIn();
    }

    private void OnZoomOutClicked(object? sender, EventArgs e)
    {
        Map.ZoomOut();
    }

    private void OnFitToScreenClicked(object? sender, EventArgs e)
    {
        Map.FitToScreen();
    }

    private void Release()
    {
        if (_isReleased)
        {
            return;
        }

        _isReleased = true;
        Map.StateChanged -= OnMapStateChanged;
        Map.Release();
        _scope.Dispose();
    }
}
