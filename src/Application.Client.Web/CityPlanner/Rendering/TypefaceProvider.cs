using System.Globalization;
using Ingweland.Fog.Application.Client.Web.CityPlanner.Abstractions;
using Ingweland.Fog.Application.Client.Web.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace Ingweland.Fog.Application.Client.Web.CityPlanner.Rendering;

public class TypefaceProvider : ITypefaceProvider
{
    private const string MAIN_FONT = "Roboto_Condensed-Regular";
    private readonly IAssetUrlProvider _assetUrlProvider;
    private readonly HttpClient _httpClient;
    private readonly Lazy<Task> _initializeOnce;
    private readonly ILogger<TypefaceProvider> _logger;

    public TypefaceProvider(HttpClient httpClient,
        IAssetUrlProvider assetUrlProvider, ILogger<TypefaceProvider> logger)
    {
        _httpClient = httpClient;
        _assetUrlProvider = assetUrlProvider;
        _logger = logger;

        _initializeOnce = new Lazy<Task>(InitializeCoreAsync);
    }

    public SKTypeface MainTypeface { get; private set; } = null!;

    public Task InitializeAsync()
    {
        return _initializeOnce.Value;
    }

    private async Task InitializeCoreAsync()
    {
        var url = _assetUrlProvider.GetFontUrl(MAIN_FONT, CultureInfo.CurrentCulture.Name);
        try
        {
            await using var fontStream = await _httpClient.GetStreamAsync(url);
            using var memoryStream = new MemoryStream();
            await fontStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            MainTypeface = SKTypeface.FromStream(memoryStream);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error setting up {font} font for {target}. Falling back to the default.",
                MAIN_FONT, nameof(BuildingRenderer));
            MainTypeface = SKTypeface.CreateDefault();
        }
    }
}
