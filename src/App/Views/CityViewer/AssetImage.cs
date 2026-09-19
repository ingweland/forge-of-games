using Ingweland.Fog.App.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.App.Views.CityViewer;

/// <summary>
///     An Image for a remote asset such as an icon. Its file comes through the asset cache
///     (<see cref="AssetCacheHandler" />), so it is downloaded once and then read from disk, also after a
///     restart; MAUI's own URL images keep theirs for a day only.
/// </summary>
public class AssetImage : Image
{
    public static readonly BindableProperty UrlProperty = BindableProperty.Create(nameof(Url), typeof(string),
        typeof(AssetImage), propertyChanged: (bindable, _, _) => _ = ((AssetImage) bindable).LoadAsync());

    private int _loadVersion;

    public string? Url
    {
        get => (string?) GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }

    private async Task LoadAsync()
    {
        // A result for a Url that has changed since is dropped.
        var version = ++_loadVersion;
        Source = null;

        var url = Url;
        var services = IPlatformApplication.Current?.Services;
        if (string.IsNullOrEmpty(url) || services == null)
        {
            return;
        }

        try
        {
            var bytes = await services.GetRequiredService<IHttpClientFactory>()
                .CreateClient(AssetCacheHandler.HTTP_CLIENT_NAME)
                .GetByteArrayAsync(url);
            if (version == _loadVersion)
            {
                Source = ImageSource.FromStream(() => new MemoryStream(bytes));
            }
        }
        catch (Exception e)
        {
            services.GetService<ILogger<AssetImage>>()?.LogWarning(e, "Could not load image {url}", url);
        }
    }
}
