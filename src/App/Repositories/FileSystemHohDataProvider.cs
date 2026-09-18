using Ingweland.Fog.Application.Client.Web.Repositories.Abstractions;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Ingweland.Fog.Models.Hoh.Entities;
using Ingweland.Fog.Shared.Helpers.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.App.Repositories;

/// <summary>
///     MAUI counterpart of the Blazor client's IndexedDbHohDataProvider: the same download-once,
///     cache-by-version flow, backed by the app data directory instead of IndexedDB.
/// </summary>
public class FileSystemHohDataProvider(
    IProtobufSerializer protobufSerializer,
    IHohDataService hohDataService,
    ILogger<FileSystemHohDataProvider> logger)
    : HohDataProviderBase<Data>(logger), IHohDataProvider
{
    private readonly HohDataFileCache _cache = new("core");

    protected override async Task<Data> LoadAsync(string version)
    {
        var cached = await _cache.TryReadAsync(version);
        if (cached != null)
        {
            try
            {
                return protobufSerializer.DeserializeFromBytes<Data>(cached);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Cached Hoh core data {version} could not be deserialized. Re-downloading.",
                    version);
            }
        }

        var (dataVersion, data) = await hohDataService.GetHohCoreDataAsync();
        if (data == null)
        {
            throw new InvalidOperationException("Could not load Hoh core data.");
        }

        await _cache.TryWriteAsync(dataVersion, data);
        return protobufSerializer.DeserializeFromBytes<Data>(data);
    }
}
