using Ingweland.Fog.App.Views;

namespace Ingweland.Fog.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // The website's menu footer (NavBar.razor) doesn't localize this line either.
        CopyrightLabel.Text = $"© {DateTime.Now.Year} Forge of Games";
        Routing.RegisterRoute(nameof(CityViewerPage), typeof(CityViewerPage));
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
}
