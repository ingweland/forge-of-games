using Ingweland.Fog.App.ViewModels.CityViewer;
using Ingweland.Fog.Application.Client.Web.CityPlanner.Abstractions;
using Ingweland.Fog.Application.Client.Web.CityPlanner.Stats;
using Ingweland.Fog.Application.Client.Web.Providers.Interfaces;
using Ingweland.Fog.Application.Core.Constants;
using Ingweland.Fog.Models.Fog.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Shapes;
using SkiaSharp.Views.Maui;
using Size = System.Drawing.Size;

namespace Ingweland.Fog.App.Views;

/// <summary>
///     Read-only city map: the MAUI counterpart of WebApp.Client's LayoutViewerComponentBase and
///     CityViewerComponentBase. Rendering, pan, pinch-zoom and hit-testing all live in the shared
///     Application.Client.Web city planner code; this page only hosts the canvas and forwards input.
///     Next to the map it shows the city properties and, after a tap, the building's (CityViewerComponent on
///     wide windows, CityMobileViewerComponent's dialog and analytics toggle on narrow ones).
/// </summary>
public partial class CityViewerPage : ContentPage, IQueryAttributable
{
    public const string CITY_QUERY_KEY = "City";

    // Same step as the web toolbar; the shared code turns ±100 into a 10% zoom.
    private const float ZOOM_STEP = 100;

    private readonly ICityPlanner _cityPlanner;
    private readonly ICityViewerInteractionManager _interactionManager;
    private readonly ILogger<CityViewerPage> _logger;
    private readonly IServiceScope _scope;
    private readonly string _storageIconUrl;
    private readonly string _totalAreaIconUrl;

    private Size _canvasSize = Size.Empty;
    private bool _fitOnPaint = true;
    private bool _initializationFailed;
    private bool _initializationStarted;
    private bool _isCityPropertiesToggled;
    private bool _isInitialized;
    private bool _isReleased;
    private bool? _isWide;
    private CityMapEntityViewModel? _shownEntity;

    public CityViewerPage(IServiceScopeFactory serviceScopeFactory, ILogger<CityViewerPage> logger)
    {
        _logger = logger;

        // The city planner services are scoped, and the web gets a fresh set per page visit. MAUI would
        // resolve them from the window scope instead, sharing pan/zoom and selection across visits, so
        // each viewer owns its own scope.
        _scope = serviceScopeFactory.CreateScope();
        _cityPlanner = _scope.ServiceProvider.GetRequiredService<ICityPlanner>();
        _interactionManager = _scope.ServiceProvider.GetRequiredService<ICityViewerInteractionManager>();

        // The web builds these two icon URLs in its razor markup rather than in the view models.
        var assetUrlProvider = _scope.ServiceProvider.GetRequiredService<IAssetUrlProvider>();
        _storageIconUrl = assetUrlProvider.GetHohIconUrl("icon_storage");
        _totalAreaIconUrl = assetUrlProvider.GetHohIconUrl("icon_flat_expansion");

        InitializeComponent();
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
                _initializationFailed = true;
                UpdatePropertiesVisibility();
            }

            return;
        }

        if (_isReleased)
        {
            return;
        }

        _cityPlanner.StateHasChanged += OnCityPlannerStateHasChanged;
        _isInitialized = true;
        LoadingIndicator.IsRunning = false;
        Toolbar.IsVisible = true;

        // Bound once: in the read-only viewer every selection rebuilds this view model with the same
        // content, and rebinding would reset the card's collapsed sections.
        var cityProperties = _cityPlanner.CityMapState.CityPropertiesViewModel;
        if (cityProperties != null)
        {
            CityCard.BindingContext = new CityPropertiesPanelModel(cityProperties,
                _cityPlanner.CityMapState.CityWonder?.WonderName, _storageIconUrl, _totalAreaIconUrl);
        }

        UpdatePropertiesVisibility();
        CanvasView.InvalidateSurface();
    }

    private void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        _canvasSize = new Size(e.Info.Width, e.Info.Height);

        if (!_isInitialized || _isReleased)
        {
            canvas.Clear(BackgroundColor.ToSKColor());
            return;
        }

        // Fit on the first paint after initialization, not before: that is when the real canvas size
        // is known.
        if (_fitOnPaint)
        {
            _interactionManager.FitToScreen(_canvasSize);
            _fitOnPaint = false;
        }

        _interactionManager.TransformMapArea(canvas);
        _cityPlanner.RenderScene(canvas);
    }

    private void OnCanvasViewTouch(object? sender, SKTouchEventArgs e)
    {
        if (!_isInitialized || _isReleased)
        {
            return;
        }

        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                _interactionManager.OnPointerDown(e.Id, e.Location.X, e.Location.Y);
                break;
            // Hover moves arrive too; only pan while a finger or button is down, as the web does with
            // `Buttons == 1`.
            case SKTouchAction.Moved when e.InContact:
                _interactionManager.OnPointerMove(e.Id, e.Location.X, e.Location.Y);
                CanvasView.InvalidateSurface();
                break;
            case SKTouchAction.Released:
            case SKTouchAction.Cancelled:
                _interactionManager.OnPointerUp(e.Id, e.Location.X, e.Location.Y);
                CanvasView.InvalidateSurface();
                SyncSelection();
                break;
            case SKTouchAction.WheelChanged:
                // The shared zoom expects the browser's deltaY sign (negative = zoom in), while
                // WheelDelta is positive when the wheel turns away from the user.
                _interactionManager.Zoom(e.Location.X, e.Location.Y, -e.WheelDelta);
                CanvasView.InvalidateSurface();
                break;
        }

        // Without this, platforms stop delivering the rest of the gesture after Pressed.
        e.Handled = true;
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0)
        {
            return;
        }

        var isWide = width >= FogConstants.CITY_PLANNER_REQUIRED_SCREEN_WIDTH;
        if (isWide != _isWide)
        {
            _isWide = isWide;
            ApplyLayoutMode(isWide);
        }
    }

    // Wide: a side panel in column 1 across both rows. Narrow: a bottom sheet over row 1 of the map, whose
    // share of the height comes from the grid's star rows. Switching doesn't re-fit the map.
    private void ApplyLayoutMode(bool isWide)
    {
        Grid.SetColumn(PropertiesHost, isWide ? 1 : 0);
        Grid.SetRow(PropertiesHost, isWide ? 0 : 1);
        Grid.SetRowSpan(PropertiesHost, isWide ? 2 : 1);
        PropertiesHost.WidthRequest = isWide ? 300 : -1;
        PropertiesHost.StrokeShape = isWide
            ? new Rectangle()
            : new RoundRectangle {CornerRadius = new CornerRadius(16, 16, 0, 0)};
        ClosePropertiesButton.IsVisible = !isWide;
        AnalyticsButton.IsVisible = !isWide;
        UpdatePropertiesVisibility();
    }

    private void UpdatePropertiesVisibility()
    {
        var isWide = _isWide ?? true;
        var hasEntity = _shownEntity != null;

        EntityCard.IsVisible = hasEntity;
        // The sheet shows one card at a time, the building taking precedence over the city properties.
        CityCard.IsVisible = CityCard.BindingContext != null &&
            (isWide || (!hasEntity && _isCityPropertiesToggled));
        // The side panel is there from the start, so the canvas has its final width before the map is fitted.
        PropertiesHost.IsVisible = isWide ? !_initializationFailed : EntityCard.IsVisible || CityCard.IsVisible;

        if (_isCityPropertiesToggled)
        {
            AnalyticsButton.BackgroundColor = ThemeResources.Color("FogPrimaryColor");
        }
        else
        {
            AnalyticsButton.ClearValue(BackgroundColorProperty);
        }
    }

    // Runs after every pointer-up. Selection only changes through taps, and tapping empty map deselects
    // without raising ICityPlanner.StateHasChanged.
    private void SyncSelection()
    {
        var entity = _cityPlanner.CityMapState.SelectedEntityViewModel;
        if (ReferenceEquals(entity, _shownEntity))
        {
            return;
        }

        _shownEntity = entity;
        EntityCard.BindingContext = entity == null ? null : new CityMapEntityPanelModel(entity, _storageIconUrl);
        UpdatePropertiesVisibility();

        if (entity != null)
        {
            // Not awaited: the host may have only just become visible and not be laid out yet.
            Dispatcher.Dispatch(() => _ = PropertiesScroll.ScrollToAsync(0, 0, false));
        }
    }

    private void OnAnalyticsClicked(object? sender, EventArgs e)
    {
        if (_shownEntity != null)
        {
            // Asked for the city properties while a building's are shown: switch the sheet over.
            _isCityPropertiesToggled = true;
            DeselectEntity();
            return;
        }

        _isCityPropertiesToggled = !_isCityPropertiesToggled;
        UpdatePropertiesVisibility();
    }

    private void OnClosePropertiesClicked(object? sender, EventArgs e)
    {
        if (_shownEntity != null)
        {
            DeselectEntity();
            return;
        }

        _isCityPropertiesToggled = false;
        UpdatePropertiesVisibility();
    }

    private void DeselectEntity()
    {
        _cityPlanner.DeselectAll();
        // DeselectAll raises no ICityPlanner.StateHasChanged, so repaint and resync here.
        CanvasView.InvalidateSurface();
        SyncSelection();
    }

    private void OnZoomInClicked(object? sender, EventArgs e)
    {
        ZoomAtCenter(-ZOOM_STEP);
    }

    private void OnZoomOutClicked(object? sender, EventArgs e)
    {
        ZoomAtCenter(ZOOM_STEP);
    }

    private void OnFitToScreenClicked(object? sender, EventArgs e)
    {
        if (!_isInitialized || _canvasSize.IsEmpty)
        {
            return;
        }

        _interactionManager.FitToScreen(_canvasSize);
        CanvasView.InvalidateSurface();
    }

    private void ZoomAtCenter(float delta)
    {
        if (!_isInitialized || _canvasSize.IsEmpty)
        {
            return;
        }

        _interactionManager.Zoom(_canvasSize.Width / 2f, _canvasSize.Height / 2f, delta);
        CanvasView.InvalidateSurface();
    }

    private void OnCityPlannerStateHasChanged()
    {
        if (_isReleased)
        {
            return;
        }

        if (Dispatcher.IsDispatchRequired)
        {
            Dispatcher.Dispatch(CanvasView.InvalidateSurface);
        }
        else
        {
            CanvasView.InvalidateSurface();
        }
    }

    private void Release()
    {
        if (_isReleased)
        {
            return;
        }

        _isReleased = true;
        _cityPlanner.StateHasChanged -= OnCityPlannerStateHasChanged;
        _scope.Dispose();
    }
}
