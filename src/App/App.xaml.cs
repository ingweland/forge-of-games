namespace Ingweland.Fog.App;

// The base type is fully qualified on purpose: this namespace sits under Ingweland.Fog, so a bare
// `Application` binds to the Ingweland.Fog.Application namespace rather than the MAUI type.
public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
