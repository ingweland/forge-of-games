using Ingweland.Fog.App.Services.Abstractions;
using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.App.Views;

public partial class CityViewerHomePage : ContentPage
{
    private const string LAST_SHARE_ID_KEY = "CityViewerHome.LastShareId";

    private readonly IFogSharingUiService _fogSharingUiService;
    private readonly IHohDataInitializationService _hohDataInitializationService;
    private readonly ILogger<CityViewerHomePage> _logger;
    private bool _dataRequested;

    public CityViewerHomePage(IHohDataInitializationService hohDataInitializationService,
        IFogSharingUiService fogSharingUiService, ILogger<CityViewerHomePage> logger)
    {
        _hohDataInitializationService = hohDataInitializationService;
        _fogSharingUiService = fogSharingUiService;
        _logger = logger;

        InitializeComponent();
        ShareLinkEntry.Text = Preferences.Get(LAST_SHARE_ID_KEY, string.Empty);
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
            await _hohDataInitializationService.InitializeAsync();
            SetBusy(false);
        }
        catch (Exception e)
        {
            // Not retryable in-process: HohDataProviderBase keeps a failed load. Restart the app.
            _logger.LogError(e, "Could not load Hoh data");
            BusyIndicator.IsRunning = false;
            ShowStatus(e.Message);
        }
    }

    private async void OnLoadClicked(object? sender, EventArgs e)
    {
        var shareId = ParseShareId(ShareLinkEntry.Text);
        if (shareId == null || !LoadButton.IsEnabled)
        {
            return;
        }

        SetBusy(true);
        try
        {
            var city = await _fogSharingUiService.FetchCityAsync(shareId);
            if (city == null)
            {
                ShowStatus(string.Format(FogResource.ImportInGameData_NotFound, shareId));
                return;
            }

            Preferences.Set(LAST_SHARE_ID_KEY, shareId);
            await Shell.Current.GoToAsync(nameof(CityViewerPage),
                new ShellNavigationQueryParameters {{CityViewerPage.CITY_QUERY_KEY, city}});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load shared city {ShareId}", shareId);
            ShowStatus(ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool isBusy)
    {
        BusyIndicator.IsRunning = isBusy;
        LoadButton.IsEnabled = !isBusy;
        ShareLinkEntry.IsEnabled = !isBusy;
        if (isBusy)
        {
            StatusLabel.IsVisible = false;
        }
    }

    private void ShowStatus(string message)
    {
        StatusLabel.Text = message;
        StatusLabel.IsVisible = true;
    }

    /// <summary>
    ///     Accepts either a bare share id or a share link such as
    ///     https://forgeofgames.com/city-planner/shares/{shareId}.
    /// </summary>
    private static string? ParseShareId(string? input)
    {
        var text = input?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri))
        {
            return text;
        }

        var lastSegment = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        return lastSegment == null ? null : Uri.UnescapeDataString(lastSegment);
    }
}
