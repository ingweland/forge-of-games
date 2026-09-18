using Ingweland.Fog.App.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Ingweland.Fog.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        using var settings = typeof(MauiProgram).Assembly.GetManifestResourceStream("appsettings.json") ??
            throw new InvalidOperationException("Embedded appsettings.json not found.");
        builder.Configuration.AddJsonStream(settings);

        AppCulture.Apply();

        builder.Services.AddAppServices(builder.Configuration);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
