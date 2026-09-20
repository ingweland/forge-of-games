using Ingweland.Fog.App.Services.Abstractions;
using Ingweland.Fog.App.ViewModels.Guides;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.App.Views;

public partial class AlliedCultureCityGuidesPage : ContentPage
{
    // The world the website's page asks for its events (AlliedCultureCityGuides.razor.cs).
    private const string WORLD_ID = "un1";

    private readonly IAlliedCultureCityGuidesUiService _guidesUiService;
    private readonly IHohDataInitializationService _hohDataInitializationService;
    private readonly ILogger<AlliedCultureCityGuidesPage> _logger;
    private readonly IServiceScope _scope;
    private bool _dataRequested;

    public AlliedCultureCityGuidesPage(IServiceScopeFactory serviceScopeFactory,
        IHohDataInitializationService hohDataInitializationService,
        ILogger<AlliedCultureCityGuidesPage> logger)
    {
        _hohDataInitializationService = hohDataInitializationService;
        _logger = logger;

        // AlliedCultureCityGuidesUiService is scoped and keeps the calendar it built. MAUI's scopes last as long as
        // the window, so its own scope keeps the calendar of the page the side menu rebuilds for a new language out
        // of the old one's. It lives as long as the page, which only a new shell replaces.
        _scope = serviceScopeFactory.CreateScope();
        _guidesUiService = _scope.ServiceProvider.GetRequiredService<IAlliedCultureCityGuidesUiService>();

        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_dataRequested)
        {
            return;
        }

        _dataRequested = true;
        try
        {
            // The guides are built from the game's own data, which the city viewer's start page also waits for.
            await _hohDataInitializationService.InitializeAsync();

            var calendarTask = _guidesUiService.GetCalendarAsync(WORLD_ID);
            var guidesTask = _guidesUiService.GetGuidesAsync();
            await Task.WhenAll(calendarTask, guidesTask);

            var groups = guidesTask.Result;
            var guides = groups.SelectMany(x => x.Guides).ToList();
            var calendar = calendarTask.Result
                .Select(x => new CalendarCardModel(x, guides.FirstOrDefault(g => g.Wonder.Id == x.WonderId)))
                .ToList();

            BindableLayout.SetItemsSource(CalendarList, calendar);
            BindableLayout.SetItemsSource(GuideGroups, groups);
            BusyIndicator.IsRunning = false;
        }
        catch (Exception e)
        {
            // Not retryable in-process: HohDataProviderBase keeps a failed load. Restart the app.
            _logger.LogError(e, "Could not load the allied culture city guides");
            BusyIndicator.IsRunning = false;
            StatusLabel.Text = e.Message;
            StatusLabel.IsVisible = true;
        }
    }
}
