using System.Globalization;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Ingweland.Fog.Models.Hoh.Entities;
using Ingweland.Fog.Shared.Helpers.Interfaces;
using Ingweland.Fog.Shared.Localization;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.App.Repositories;

/// <summary>
///     MAUI counterpart of the Blazor client's IndexedDbHohLocalizationDataProvider. Supplies the
///     in-game strings (building names, wonder names, ...) the city map draws, in the app's language.
///     Each language is downloaded once per data version and then read from disk. Not a
///     HohDataProviderBase, which loads once: the website reloads the page for a new language, while
///     the app loads the strings again (HohDataInitializationService).
/// </summary>
public class FileSystemHohLocalizationDataProvider(
    IProtobufSerializer protobufSerializer,
    IHohDataService hohDataService,
    ILogger<FileSystemHohLocalizationDataProvider> logger) : IDataProvider, IHohLocalizationDataProvider
{
    private readonly HohDataFileCache _cache = new("localization");
    private IDictionary<string, LocalizationData> _data = new Dictionary<string, LocalizationData>();

    public async Task InitializeAsync(string version)
    {
        // HohGameLocalizationService looks the data up by CultureInfo.CurrentCulture.Name, which
        // AppCulture sets to a supported culture.
        var cultureCode = CultureInfo.CurrentCulture.Name;
        var data = await LoadAsync(version, cultureCode);

        // The language can change while this loads. The load for the new language sets its own data.
        if (cultureCode == CultureInfo.CurrentCulture.Name)
        {
            _data = data;
        }
    }

    public IDictionary<string, LocalizationData> GetData()
    {
        return _data;
    }

    private async Task<IDictionary<string, LocalizationData>> LoadAsync(string version, string cultureCode)
    {
        var cached = await _cache.TryReadAsync(version, cultureCode);
        if (cached != null)
        {
            try
            {
                return Build(cultureCode, protobufSerializer.DeserializeFromBytes<LocalizationData>(cached));
            }
            catch (Exception e)
            {
                logger.LogWarning(e,
                    "Cached Hoh localization data {version}/{culture} could not be deserialized. Re-downloading.",
                    version, cultureCode);
            }
        }

        var (dataVersion, data) = await hohDataService.GetHohLocalizationDataAsync(cultureCode);
        if (data == null)
        {
            throw new InvalidOperationException($"Could not load Hoh localization data for {cultureCode}.");
        }

        await _cache.TryWriteAsync(dataVersion, cultureCode, data);
        return Build(cultureCode, protobufSerializer.DeserializeFromBytes<LocalizationData>(data));
    }

    private static Dictionary<string, LocalizationData> Build(string cultureCode, LocalizationData data)
    {
        // HohGameLocalizationDataRepository indexes data[DefaultCulture] directly when the requested
        // culture is missing, so seed that key too rather than risk a KeyNotFoundException if the
        // ambient culture ever differs from the one we downloaded.
        var result = new Dictionary<string, LocalizationData> {{cultureCode, data}};
        result.TryAdd(HohSupportedCultures.DefaultCulture, data);
        return result;
    }
}
