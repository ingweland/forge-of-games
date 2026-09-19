namespace Ingweland.Fog.App;

// The base type is fully qualified on purpose: this namespace sits under Ingweland.Fog, so a bare
// `Application` binds to the Ingweland.Fog.Application namespace rather than the MAUI type.
public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();
        // Light only, like the website: the palette and the property panels have no dark variant.
        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        window.Created += (_, _) => UpdateHairlineThickness(window.DisplayDensity);
        window.DisplayDensityChanged += (_, e) => UpdateHairlineThickness(e.DisplayDensity);
        return window;
    }

    // Browsers draw the website's 1px borders as whole device pixels, at least one (CSS "snap as a border width").
    // A 1-unit line would be 1.5 pixels at 150% scaling and blur across two pixel rows.
    private void UpdateHairlineThickness(float density)
    {
        if (density > 0)
        {
            Resources["FogHairlineThickness"] = Math.Max(1, Math.Floor(density)) / density;
        }
    }
}
