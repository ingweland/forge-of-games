using System.Diagnostics;
using Ingweland.Fog.Application.Client.Web.Data.Entities;
using Ingweland.Fog.Application.Client.Web.Repositories.Abstractions;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Ingweland.Fog.Models.Hoh.Entities;
using Ingweland.Fog.Shared.Helpers.Interfaces;
using Ingweland.Fog.WebApp.Client.Repositories.Abstractions;

namespace Ingweland.Fog.WebApp.Client.Repositories;

public class IndexedDbHohDataProvider(
    IProtobufSerializer protobufSerializer,
    IHohDataService hohDataService,
    IFogLocalDbContext fogLocalDbContext,
    ILogger<IndexedDbHohDataProvider> logger)
    : HohDataProviderBase<Data>(logger), IHohDataProvider
{
    protected override async Task<Data> LoadAsync(string version)
    {
        var sw = new Stopwatch();
        sw.Start();
        var key = await fogLocalDbContext.HohCoreData.GetKeyAsync<string, string>(version);
        sw.Stop();
        logger.LogDebug("Hoh core data key loaded: {elapsed}", sw.Elapsed);
        if (key != null)
        {
            var record = await fogLocalDbContext.HohCoreData.GetAsync<string, HohCoreData>(version);
            sw.Stop();
            logger.LogDebug("Hoh core data loaded: {elapsed}", sw.Elapsed);
            sw.Restart();
            var result = protobufSerializer.DeserializeFromBytes<Data>(record!.Data);
            logger.LogDebug("Hoh core data deserialized: {elapsed}", sw.Elapsed);
            sw.Stop();
            return result;
        }

        sw.Restart();
        var (dataVersion, data) = await hohDataService.GetHohCoreDataAsync();
        sw.Stop();
        logger.LogDebug("Hoh core data downloaded: {elapsed}", sw.Elapsed);
        if (data != null)
        {
            sw.Restart();
            await fogLocalDbContext.HohCoreData.ClearStoreAsync();
            sw.Stop();
            logger.LogDebug("Hoh core data cleared: {elapsed}", sw.Elapsed);
            sw.Restart();
            await fogLocalDbContext.HohCoreData.AddAsync(new HohCoreData
            {
                Id = dataVersion,
                Data = data,
            });
            sw.Stop();
            logger.LogDebug("Hoh core data added: {elapsed}", sw.Elapsed);
            sw.Restart();
            var result = protobufSerializer.DeserializeFromBytes<Data>(data);
            sw.Stop();
            logger.LogDebug("Hoh core data deserialized: {elapsed}", sw.Elapsed);
            return result;
        }

        throw new Exception("Could not load Hoh core data");
    }
}
