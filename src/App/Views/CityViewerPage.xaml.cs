using Ingweland.Fog.Application.Client.Web.CityPlanner.Abstractions;
using Ingweland.Fog.Models.Fog.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui;
using Size = System.Drawing.Size;

namespace Ingweland.Fog.App.Views;

/// <summary>
///     Read-only city map: the MAUI counterpart of WebApp.Client's LayoutViewerComponentBase and
///     CityViewerComponentBase. Rendering, pan, pinch-zoom and hit-testing all live in the shared
///     Application.Client.Web city planner code; this page only hosts the canvas and forwards input.
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

    private Size _canvasSize = Size.Empty;
    private bool _fitOnPaint = true;
    private bool _initializationStarted;
    private bool _isInitialized;
    private bool _isReleased;

    public CityViewerPage(IServiceScopeFactory serviceScopeFactory, ILogger<CityViewerPage> logger)
    {
        _logger = logger;

        // The city planner services are scoped, and the web gets a fresh set per page visit. MAUI would
        // resolve them from the window scope instead, sharing pan/zoom and selection across visits, so
        // each viewer owns its own scope.
        _scope = serviceScopeFactory.CreateScope();
        _cityPlanner = _scope.ServiceProvider.GetRequiredService<ICityPlanner>();
        _interactionManager = _scope.ServiceProvider.GetRequiredService<ICityViewerInteractionManager>();

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
