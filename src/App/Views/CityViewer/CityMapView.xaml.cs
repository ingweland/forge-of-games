using Ingweland.Fog.App.ViewModels.CityViewer;
using Ingweland.Fog.Application.Client.Web.CityPlanner;
using Ingweland.Fog.Application.Client.Web.CityPlanner.Abstractions;
using Ingweland.Fog.Application.Client.Web.CityPlanner.Stats;
using Ingweland.Fog.Application.Client.Web.Providers.Interfaces;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp.Views.Maui;
using Size = System.Drawing.Size;

namespace Ingweland.Fog.App.Views.CityViewer;

/// <summary>
///     The city map: what LayoutViewerComponentBase and the property panels around it are on the website.
///     Both the standalone viewer and a guide's layout item host one of these over the city planner in their
///     own service scope; each keeps its own toolbar and drives this through
///     <see cref="ZoomIn" />, <see cref="ZoomOut" />, <see cref="FitToScreen" /> and
///     <see cref="ToggleCityProperties" />.
/// </summary>
public partial class CityMapView : ContentView
{
    // Same step as the web toolbar; the shared code turns ±100 into a 10% zoom.
    private const float ZOOM_STEP = 100;

    private BuildingCostCalculatorView? _buildingCost;
    private ICityPlanner? _cityPlanner;
    private ICityViewerInteractionManager? _interactionManager;
    private CityPlannerSettings? _settings;
    private string _storageIconUrl = string.Empty;
    private string _totalAreaIconUrl = string.Empty;
    private WonderCostCalculatorView? _wonderCost;

    private Size _canvasSize = Size.Empty;
    private bool _calculatorsTabSelected;
    private bool _fitOnPaint = true;
    private bool _initializationFailed;
    private bool _isCityPropertiesToggled;
    private bool _isInitialized;
    private bool _isReleased;
    private bool? _isWide;
    private CityMapEntityViewModel? _shownEntity;

    public CityMapView()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Raised when the panel opens or closes, so a page can restyle its toolbar and move out of the way.
    ///     Not raised by <see cref="ApplyLayoutMode" />, which a page calls from its own layout pass.
    /// </summary>
    public event Action? StateChanged;

    /// <summary>
    ///     Whether the city properties were asked for, which is what the analytics button shows.
    /// </summary>
    public bool IsCityPropertiesToggled => _isCityPropertiesToggled;

    /// <summary>
    ///     Whether the panel is over the map rather than beside it, i.e. the sheet is covering it.
    /// </summary>
    public bool IsPanelOpen => _isWide == false && PropertiesHost.IsVisible;

    /// <summary>
    ///     Takes the city planner and everything drawn around it out of the page's own service scope, which is
    ///     the scope the planner being rendered belongs to.
    /// </summary>
    public void Initialize(IServiceProvider scopeServices)
    {
        if (_isInitialized || _isReleased)
        {
            return;
        }

        _cityPlanner = scopeServices.GetRequiredService<ICityPlanner>();
        _interactionManager = scopeServices.GetRequiredService<ICityViewerInteractionManager>();
        _settings = scopeServices.GetRequiredService<CityPlannerSettings>();

        // The web builds these two icon URLs in its razor markup rather than in the view models.
        var assetUrlProvider = scopeServices.GetRequiredService<IAssetUrlProvider>();
        _storageIconUrl = assetUrlProvider.GetHohIconUrl("icon_storage");
        _totalAreaIconUrl = assetUrlProvider.GetHohIconUrl("icon_flat_expansion");

        var toolsUiService = scopeServices.GetRequiredService<IToolsUiService>();
        _buildingCost =
            new BuildingCostCalculatorView(scopeServices.GetRequiredService<ICityUiService>(), toolsUiService);
        _wonderCost = new WonderCostCalculatorView(toolsUiService);
        PanelStack.Insert(1, _buildingCost);
        PanelStack.Add(_wonderCost);

        _cityPlanner.StateHasChanged += OnCityPlannerStateHasChanged;
        // CityStrategyViewerComponentBase repaints on this too: diff mode is what changes under it.
        _settings.StateChanged += OnSettingsChanged;

        _isInitialized = true;
    }

    /// <summary>
    ///     The planner has just been initialized on a city, so everything built from its state is stale. A
    ///     guide calls this for every layout item it selects; the standalone viewer once.
    /// </summary>
    public void Refresh()
    {
        if (!_isInitialized || _isReleased)
        {
            return;
        }

        // The selection does not survive a new city: its view model belonged to the old one.
        _shownEntity = null;
        EntityCard.BindingContext = null;
        _buildingCost!.Clear();

        // Bound once per city: within one, every selection rebuilds this view model with the same content,
        // and rebinding would reset the card's collapsed sections.
        var state = _cityPlanner!.CityMapState;
        CityCard.BindingContext = state.CityPropertiesViewModel == null
            ? null
            : new CityPropertiesPanelModel(state.CityPropertiesViewModel, state.CityWonder?.WonderName,
                _storageIconUrl, _totalAreaIconUrl);
        _wonderCost!.Show(state.CityWonder, state.CityWonderLevel);

        UpdatePropertiesVisibility();
        CanvasView.InvalidateSurface();
        StateChanged?.Invoke();
    }

    /// <summary>
    ///     The city could not be loaded: leave the panel out rather than showing an empty one.
    /// </summary>
    public void ShowFailure()
    {
        _initializationFailed = true;
        UpdatePropertiesVisibility();
    }

    // Wide: a side panel in column 1 beside the map. Narrow: over the map and covering it, as the website's
    // mobile viewer covers it with a full dialog for a building and replaces it outright for the city.
    // Switching doesn't re-fit the map.
    public void ApplyLayoutMode(bool isWide)
    {
        _isWide = isWide;

        Grid.SetColumn(PropertiesHost, isWide ? 1 : 0);
        PropertiesHost.WidthRequest = isWide ? 300 : -1;
        ClosePropertiesButton.IsVisible = !isWide;
        // A phone shows the panel one card at a time, so it has nothing to tab between.
        TabStrip.IsVisible = isWide;
        UpdatePropertiesVisibility();
    }

    public void ZoomIn()
    {
        ZoomAtCenter(-ZOOM_STEP);
    }

    public void ZoomOut()
    {
        ZoomAtCenter(ZOOM_STEP);
    }

    public void FitToScreen()
    {
        if (!_isInitialized || _canvasSize.IsEmpty)
        {
            return;
        }

        _interactionManager!.FitToScreen(_canvasSize);
        CanvasView.InvalidateSurface();
    }

    public void ToggleCityProperties()
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
        StateChanged?.Invoke();
    }

    /// <summary>
    ///     Stops listening to the planner. Call this before the scope the planner came from is disposed.
    /// </summary>
    public void Release()
    {
        if (_isReleased)
        {
            return;
        }

        _isReleased = true;
        if (_cityPlanner != null)
        {
            _cityPlanner.StateHasChanged -= OnCityPlannerStateHasChanged;
        }

        if (_settings != null)
        {
            _settings.StateChanged -= OnSettingsChanged;
        }
    }

    private void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        _canvasSize = new Size(e.Info.Width, e.Info.Height);

        if (!_isInitialized || _isReleased)
        {
            canvas.Clear((BackgroundColor ?? Colors.Transparent).ToSKColor());
            return;
        }

        // Fit on the first paint after initialization, not before: that is when the real canvas size
        // is known. Once only, as LayoutViewerComponentBase does, so stepping between a guide's layout
        // items keeps the pan and zoom the reader set.
        if (_fitOnPaint)
        {
            _interactionManager!.FitToScreen(_canvasSize);
            _fitOnPaint = false;
        }

        _interactionManager!.TransformMapArea(canvas);
        _cityPlanner!.RenderScene(canvas);
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
                _interactionManager!.OnPointerDown(e.Id, e.Location.X, e.Location.Y);
                break;
            // Hover moves arrive too; only pan while a finger or button is down, as the web does with
            // `Buttons == 1`.
            case SKTouchAction.Moved when e.InContact:
                _interactionManager!.OnPointerMove(e.Id, e.Location.X, e.Location.Y);
                CanvasView.InvalidateSurface();
                break;
            case SKTouchAction.Released:
            case SKTouchAction.Cancelled:
                _interactionManager!.OnPointerUp(e.Id, e.Location.X, e.Location.Y);
                CanvasView.InvalidateSurface();
                SyncSelection();
                break;
            case SKTouchAction.WheelChanged:
                // The shared zoom expects the browser's deltaY sign (negative = zoom in), while
                // WheelDelta is positive when the wheel turns away from the user.
                _interactionManager!.Zoom(e.Location.X, e.Location.Y, -e.WheelDelta);
                CanvasView.InvalidateSurface();
                break;
        }

        // Without this, platforms stop delivering the rest of the gesture after Pressed.
        e.Handled = true;
    }

    private void UpdatePropertiesVisibility()
    {
        var isWide = _isWide ?? true;
        var hasEntity = _shownEntity != null;
        // Wide windows split the panel in two and the open tab decides; a phone runs both groups together
        // and shows one card at a time, the building taking precedence over the city properties.
        var showProperties = !isWide || !_calculatorsTabSelected;
        var showCalculators = !isWide || _calculatorsTabSelected;

        EntityCard.IsVisible = hasEntity && showProperties;
        CityCard.IsVisible = CityCard.BindingContext != null && showProperties &&
            (isWide || (!hasEntity && _isCityPropertiesToggled));
        // On a phone each calculator hangs under its own card and goes wherever that card goes; on the
        // calculators tab both stand on their own, and each hides itself when it has nothing to cost.
        if (_buildingCost != null)
        {
            _buildingCost.IsHostVisible = isWide ? showCalculators : EntityCard.IsVisible;
        }

        if (_wonderCost != null)
        {
            _wonderCost.IsHostVisible = isWide ? showCalculators : CityCard.IsVisible;
        }

        PropertiesTabSlider.Color = SliderColor(!_calculatorsTabSelected);
        CalculatorsTabSlider.Color = SliderColor(_calculatorsTabSelected);

        // The side panel is there from the start, so the canvas has its final width before the map is fitted.
        PropertiesHost.IsVisible = isWide ? !_initializationFailed : EntityCard.IsVisible || CityCard.IsVisible;
    }

    // Kept in the layout rather than hidden, so the icon beside it does not shift as the tab changes, and
    // painted in the strip's own color when it is not the open tab: a transparent BoxView comes out black.
    private static Color SliderColor(bool isSelected)
    {
        return ThemeResources.Color(isSelected ? "FogTabSliderColor" : "FogTabStripColor");
    }

    private void OnPropertiesTabClicked(object? sender, EventArgs e)
    {
        SelectTab(false);
    }

    private void OnCalculatorsTabClicked(object? sender, EventArgs e)
    {
        SelectTab(true);
    }

    private void SelectTab(bool calculators)
    {
        if (_calculatorsTabSelected == calculators)
        {
            return;
        }

        _calculatorsTabSelected = calculators;
        UpdatePropertiesVisibility();
        // The two tabs scroll as one, so the new one would otherwise open part-way down.
        _ = PropertiesScroll.ScrollToAsync(0, 0, false);
    }

    // Runs after every pointer-up. Selection only changes through taps, and tapping empty map deselects
    // without raising ICityPlanner.StateHasChanged.
    private void SyncSelection()
    {
        var entity = _cityPlanner!.CityMapState.SelectedEntityViewModel;
        if (ReferenceEquals(entity, _shownEntity))
        {
            return;
        }

        _shownEntity = entity;
        EntityCard.BindingContext = entity == null ? null : new CityMapEntityPanelModel(entity, _storageIconUrl);
        if (entity == null)
        {
            _buildingCost!.Clear();
        }
        else
        {
            // Not awaited: the building group is read from the on-device game data, and the card is drawn
            // whether or not the cost arrives.
            _ = _buildingCost!.ShowAsync(_cityPlanner.CityMapState.InGameCityId, entity.Group, entity.Name,
                entity.Level);
        }

        UpdatePropertiesVisibility();
        StateChanged?.Invoke();

        if (entity != null)
        {
            // Not awaited: the host may have only just become visible and not be laid out yet.
            Dispatcher.Dispatch(() => _ = PropertiesScroll.ScrollToAsync(0, 0, false));
        }
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
        StateChanged?.Invoke();
    }

    private void DeselectEntity()
    {
        _cityPlanner!.DeselectAll();
        // DeselectAll raises no ICityPlanner.StateHasChanged, so repaint and resync here.
        CanvasView.InvalidateSurface();
        SyncSelection();
    }

    private void ZoomAtCenter(float delta)
    {
        if (!_isInitialized || _canvasSize.IsEmpty)
        {
            return;
        }

        _interactionManager!.Zoom(_canvasSize.Width / 2f, _canvasSize.Height / 2f, delta);
        CanvasView.InvalidateSurface();
    }

    private void OnSettingsChanged()
    {
        Repaint();
    }

    private void OnCityPlannerStateHasChanged()
    {
        Repaint();
    }

    private void Repaint()
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
}
