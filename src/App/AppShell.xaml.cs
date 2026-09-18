using Ingweland.Fog.App.Views;

namespace Ingweland.Fog.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(CityViewerPage), typeof(CityViewerPage));
    }
}
