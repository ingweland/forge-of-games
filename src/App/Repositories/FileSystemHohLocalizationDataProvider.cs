using System.Globalization;
using Ingweland.Fog.Application.Client.Web.Repositories.Abstractions;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Ingweland.Fog.Models.Hoh.Entities;
using Ingweland.Fog.Shared.Helpers.Interfaces;
using Ingweland.Fog.Shared.Localization;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.App.Repositories;

/// <summary>
///     MAUI counterpart of the Blazor client's IndexedDbHohLocalizationDataProvider. Supplies the
///     in-game strings (building names, wonder names, ...) the city map draws.
/// </summary>
public class FileSystemHohLocalizationDataProvider(
    IProtobufSerializer protobufSerializer,
    IHohDataService hohDataService,
    ILogger<FileSystemHohLocalizationDataProvider> logger)
    : HohDataProviderBase<IDictionary<string, LocalizationData>>(logger), IHohLocalizationDataProvider
{
    private readonly HohDataFileCache _cache = new("localization");

    protected override async Task<IDictionary<string, LocalizationData>> LoadAsync(string version)
    {
        // HohGameLocalizationService looks the data up by CultureInfo.CurrentCulture.Name, which
        // MauiProgram pins to a supported culture before this runs.
        var cultureCode = CultureInfo.CurrentCulture.Name;

        var cached = await _cache.TryReadAsync($"{version}_{cultureCode}");
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

        await _cache.TryWriteAsync($"{dataVersion}_{cultureCode}", data);
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
