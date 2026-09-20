using Ingweland.Fog.App.Services.Abstractions;
using Ingweland.Fog.App.Views.Guides;
using Ingweland.Fog.Application.Client.Web.CityStrategyBuilder.Abstractions;
using Ingweland.Fog.Application.Client.Web.Providers.Interfaces;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.Application.Client.Web.ViewModels;
using Ingweland.Fog.Models.Fog.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.App.Views;

/// <summary>
///     A community city guide, read only: the MAUI counterpart of CityStrategyViewerComponentBase and its two
///     views. The guide is downloaded once and cached, then handed to the shared city strategy builder service,
///     which owns the timeline and the selection; this page only arranges them.
/// </summary>
public partial class CityGuidePage : ContentPage, IQueryAttributable
{
    public const string GUIDE_QUERY_KEY = "guide";

    // What CityStrategyViewerPage calls a small screen, and so the width at which the timeline stops being a
    // panel beside the content and becomes the whole page.
    private const double WIDE_WINDOW_WIDTH = 1000;

    // The width of the website's .timeline-container.
    private const double TIMELINE_PANEL_WIDTH = 270;

    private readonly IAssetUrlProvider _assetUrlProvider;
    private readonly ICityStrategyBuilderService _builder;
    private readonly IAlliedCultureCityGuidesUiService _guidesUiService;
    private readonly IHohDataInitializationService _hohDataInitializationService;
    private readonly ILogger<CityGuidePage> _logger;
    private readonly IServiceScope _scope;

    private bool _initializationStarted;
    private bool _isNavigating;
    private bool _isReleased;
    private bool? _isWide;
    private CityStrategyTimelineItemBase? _selectedItem;
    private bool _timelineIsVisible;

    public CityGuidePage(IServiceScopeFactory serviceScopeFactory,
        IHohDataInitializationService hohDataInitializationService, ILogger<CityGuidePage> logger)
    {
        _hohDataInitializationService = hohDataInitializationService;
        _logger = logger;

        // As in CityViewerPage: the city strategy builder is transient, but the city planner and command
        // manager behind it are scoped, and MAUI resolves scoped services from the window. Its own scope keeps
        // one guide's state out of the next one's, and disposing it on the way out disposes them all.
        _scope = serviceScopeFactory.CreateScope();
        _builder = _scope.ServiceProvider.GetRequiredService<ICityStrategyBuilderService>();
        _guidesUiService = _scope.ServiceProvider.GetRequiredService<IAlliedCultureCityGuidesUiService>();
        _assetUrlProvider = _scope.ServiceProvider.GetRequiredService<IAssetUrlProvider>();

        InitializeComponent();

        Timeline.ItemSelected += OnTimelineItemSelected;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        // The shared service refuses a second InitializeAsync, and Shell may apply the query more than once.
        if (_initializationStarted || !query.TryGetValue(GUIDE_QUERY_KEY, out var value) ||
            value is not AlliedCultureCityGuideViewModel guide)
        {
            return;
        }

        _initializationStarted = true;
        Title = guide.Wonder.Name;
        _ = InitializeAsync(guide);
    }

    public static Task OpenAsync(AlliedCultureCityGuideViewModel guide)
    {
        return Shell.Current.GoToAsync(nameof(CityGuidePage),
            new ShellNavigationQueryParameters {{GUIDE_QUERY_KEY, guide}});
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

        var isWide = width >= WIDE_WINDOW_WIDTH;
        if (isWide == _isWide)
        {
            return;
        }

        _isWide = isWide;
        // The website's desktop viewer opens with its panel out and its mobile viewer on the content
        // (ViewerState.Main), so crossing the width picks where the timeline stands.
        _timelineIsVisible = isWide;
        ApplyLayoutMode();
    }

    private async Task InitializeAsync(AlliedCultureCityGuideViewModel guide)
    {
        try
        {
            // The guide is built on the game's own data, which the city viewer's start page also waits for.
            await _hohDataInitializationService.InitializeAsync();

            // Downloaded the first time, then read from the cache folder, as the website reads its own copy
            // back out of local storage.
            var strategy = await _guidesUiService.GetGuideAsync(guide.SharedDataId);
            if (_isReleased)
            {
                return;
            }

            if (strategy == null)
            {
                // The website stays on the guides list when the download fails; so does this.
                _logger.LogError("Could not download the city guide {SharedDataId}", guide.SharedDataId);
                await Shell.Current.GoToAsync("..");
                return;
            }

            await _builder.InitializeAsync(strategy, true);
            if (_isReleased)
            {
                return;
            }

            LoadingIndicator.IsRunning = false;
            Toolbar.IsVisible = true;
            Body.IsVisible = true;

            Timeline.SetItems(_builder.TimelineItems);
            ShowSelectedItem();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not open the city guide {SharedDataId}", guide.SharedDataId);
            if (_isReleased)
            {
                return;
            }

            LoadingIndicator.IsRunning = false;
            ErrorLabel.Text = e.Message;
            ErrorLabel.IsVisible = true;
        }
    }

    /// <summary>
    ///     Brings the page in line with the selected timeline item.
    /// </summary>
    private void ShowSelectedItem()
    {
        _selectedItem = _builder.SelectedTimelineItem;
        ItemTitleLabel.Text = _selectedItem?.Title ?? string.Empty;
        ItemContentHost.Content = CreateItemContent(_selectedItem);
        Timeline.Select(_selectedItem?.Id);
        ApplyLayoutMode();
    }

    // The website's two viewers are the same chain of `is` tests over these four types; the layout map and the
    // research tree join it next, and until then they leave the area empty.
    private View? CreateItemContent(CityStrategyTimelineItemBase? item)
    {
        return item switch
        {
            CityStrategyDescriptionTimelineItem description => MarkdownItemView.ForDescription(description),
            CityStrategyIntroTimelineItem intro => MarkdownItemView.ForIntro(intro, _assetUrlProvider),
            _ => null,
        };
    }

    // Wide: the timeline is a panel in column 0 and the content sits beside it, as in the website's desktop
    // viewer. Narrow: whichever of the two is showing takes both columns, and the arrows come with the content.
    private void ApplyLayoutMode()
    {
        // Before the first size, assume the panel fits: the side menu's own pages start out wide.
        var isWide = _isWide ?? true;

        Grid.SetColumnSpan(Timeline, isWide ? 1 : 2);
        Grid.SetColumn(ContentHost, isWide ? 1 : 0);
        Grid.SetColumnSpan(ContentHost, isWide ? 1 : 2);

        Timeline.WidthRequest = isWide ? TIMELINE_PANEL_WIDTH : -1;
        Timeline.IsVisible = _timelineIsVisible;
        ContentHost.IsVisible = isWide || !_timelineIsVisible;

        // The website's arrows are the mobile viewer's: with the panel there, the list is already at hand.
        NavigationButtons.IsVisible = Body.IsVisible && !isWide && !_timelineIsVisible;

        // So is the item's title. The desktop viewer prints none at all, and neither viewer prints one for an
        // intro item, whose own markdown already opens with the same title as its heading.
        ItemTitleLabel.IsVisible = !isWide && _selectedItem is not CityStrategyIntroTimelineItem;

        // .content-container's padding-bottom: the arrows float over the content, so it stops short of them.
        ItemContentHost.Margin = new Thickness(0, 0, 0, NavigationButtons.IsVisible ? 64 : 0);

        if (_timelineIsVisible)
        {
            TimelineButton.BackgroundColor = ThemeResources.Color("FogPrimaryColor");
        }
        else
        {
            TimelineButton.ClearValue(BackgroundColorProperty);
        }
    }

    private void OnTimelineClicked(object? sender, EventArgs e)
    {
        _timelineIsVisible = !_timelineIsVisible;
        ApplyLayoutMode();
    }

    private async void OnTimelineItemSelected(object? sender, string id)
    {
        if (!await SelectAsync(() => _builder.SelectTimelineItem(id)))
        {
            return;
        }

        // The website's mobile viewer goes back to the content once an item is picked.
        if (_isWide == false)
        {
            _timelineIsVisible = false;
        }

        ShowSelectedItem();
    }

    private async void OnPreviousClicked(object? sender, EventArgs e)
    {
        if (await SelectAsync(_builder.SelectPreviousItem))
        {
            ShowSelectedItem();
        }
    }

    private async void OnNextClicked(object? sender, EventArgs e)
    {
        if (await SelectAsync(_builder.SelectNextItem))
        {
            ShowSelectedItem();
        }
    }

    /// <summary>
    ///     Runs one selection at a time and reports whether the page should now redraw. Selecting a layout item
    ///     re-initializes the city planner and is not instant; the website throttles its own arrows for the same
    ///     reason, and a tap that arrives meanwhile is dropped rather than queued.
    /// </summary>
    private async Task<bool> SelectAsync(Func<Task> select)
    {
        if (_isNavigating || _isReleased)
        {
            return false;
        }

        _isNavigating = true;
        try
        {
            await select();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not select a timeline item");
            return false;
        }
        finally
        {
            _isNavigating = false;
        }

        return !_isReleased;
    }

    private void Release()
    {
        if (_isReleased)
        {
            return;
        }

        _isReleased = true;
        Timeline.ItemSelected -= OnTimelineItemSelected;
        // Disposes the city strategy builder with everything else it resolved.
        _scope.Dispose();
    }
}
