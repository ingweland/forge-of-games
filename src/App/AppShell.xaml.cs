using Ingweland.Fog.App.Services;
using Ingweland.Fog.App.Views;
using Ingweland.Fog.Application.Client.Core.Localization;

namespace Ingweland.Fog.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // The website's menu footer (NavBar.razor) doesn't localize this line either.
        CopyrightLabel.Text = $"© {DateTime.Now.Year} Forge of Games";
        LanguageLabel.Text = AppCulture.Current.Label;

        // A new shell (a language change) registers these again, which MAUI accepts for the same page types.
        Routing.RegisterRoute(nameof(CityViewerPage), typeof(CityViewerPage));
        Routing.RegisterRoute(nameof(CityGuidePage), typeof(CityGuidePage));
    }

    // Android's back button on another menu page goes to the first one (City Viewer) before leaving the app, as
    // Android's navigation guidance asks.
    protected override bool OnBackButtonPressed()
    {
        if (CurrentItem != Items[0] && Navigation.NavigationStack.Count <= 1)
        {
            CurrentItem = Items[0];
            return true;
        }

        return base.OnBackButtonPressed();
    }

    // The website reloads the page for a new language. Here a new shell reads every text again, on the same menu
    // item, and the City Viewer start page loads the game's texts in that language (HohDataInitializationService).
    private async void OnLanguageTapped(object? sender, TappedEventArgs e)
    {
        var label = await DisplayActionSheetAsync(null, FogResource.Common_Cancel, null,
            AppCulture.SupportedLocales.Select(x => x.Label).ToArray());
        var locale = AppCulture.SupportedLocales.FirstOrDefault(x => x.Label == label);
        if (locale == null || locale == AppCulture.Current || Window is not { } window)
        {
            return;
        }

        AppCulture.Change(locale);
        var shell = new AppShell();
        shell.CurrentItem = shell.Items[Items.IndexOf(CurrentItem)];
        window.Page = shell;
    }
}
