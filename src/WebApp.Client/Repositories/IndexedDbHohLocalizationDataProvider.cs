using System.Diagnostics;
using Ingweland.Fog.Application.Client.Web.Data.Entities;
using Ingweland.Fog.Application.Client.Web.Repositories.Abstractions;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Ingweland.Fog.Models.Hoh.Entities;
using Ingweland.Fog.Shared.Helpers.Interfaces;
using Ingweland.Fog.WebApp.Client.Repositories.Abstractions;
using Ingweland.Fog.WebApp.Client.Services.Abstractions;

namespace Ingweland.Fog.WebApp.Client.Repositories;

public class IndexedDbHohLocalizationDataProvider(
    IProtobufSerializer protobufSerializer,
    IHohDataService hohDataService,
    IFogLocalDbContext fogLocalDbContext,
    IClientLocaleService clientLocaleService,
    ILogger<IndexedDbHohLocalizationDataProvider> logger)
    : HohDataProviderBase<IDictionary<string, LocalizationData>>(logger),
        IHohLocalizationDataProvider
{
    protected override async Task<IDictionary<string, LocalizationData>> LoadAsync(string version)
    {
        var sw = new Stopwatch();

        sw.Start();
        var culture = await clientLocaleService.GetCurrentLocaleAsync();
        sw.Stop();
        logger.LogDebug("Hoh localization culture loaded: {elapsed}", sw.Elapsed);

        var id = $"{version}/{culture.Code}";

        sw.Restart();
        var key = await fogLocalDbContext.HohLocalizationData.GetKeyAsync<string, string>(id);
        sw.Stop();
        logger.LogDebug("Hoh localization key loaded: {elapsed}", sw.Elapsed);

        if (key != null)
        {
            sw.Restart();
            var record = await fogLocalDbContext.HohLocalizationData.GetAsync<string, HohLocalizationData>(id);
            sw.Stop();
            logger.LogDebug("Hoh localization data loaded: {elapsed}", sw.Elapsed);

            sw.Restart();
            var result = new Dictionary<string, LocalizationData>
            {
                {culture.Code, protobufSerializer.DeserializeFromBytes<LocalizationData>(record!.Data)},
            };
            sw.Stop();
            logger.LogDebug("Hoh localization data deserialized: {elapsed}", sw.Elapsed);

            return result;
        }

        sw.Restart();
        var (dataVersion, data) = await hohDataService.GetHohLocalizationDataAsync(culture.Code);
        sw.Stop();
        logger.LogDebug("Hoh localization data downloaded: {elapsed}", sw.Elapsed);

        if (data != null)
        {
            id = $"{dataVersion}/{culture.Code}";

            sw.Restart();
            var records = await fogLocalDbContext.HohLocalizationData.GetAllAsync<HohLocalizationData>();
            sw.Stop();
            logger.LogDebug("Hoh localization records loaded: {elapsed}", sw.Elapsed);

            var oldRecords = records
                .Where(x => x.Version != dataVersion || x.Id == id)
                .Select(x => x.Id)
                .ToArray();

            if (oldRecords.Length > 0)
            {
                sw.Restart();
                await fogLocalDbContext.HohLocalizationData.BatchDeleteAsync(oldRecords);
                sw.Stop();
                logger.LogDebug("Hoh localization records deleted: {elapsed}", sw.Elapsed);
            }

            sw.Restart();
            await fogLocalDbContext.HohLocalizationData.AddAsync(new HohLocalizationData
            {
                Id = id,
                Data = data,
                Version = dataVersion,
                CultureCode = culture.Code,
            });
            sw.Stop();
            logger.LogDebug("Hoh localization data added: {elapsed}", sw.Elapsed);

            sw.Restart();
            var result = new Dictionary<string, LocalizationData>
            {
                {culture.Code, protobufSerializer.DeserializeFromBytes<LocalizationData>(data)},
            };
            sw.Stop();
            logger.LogDebug("Hoh localization data deserialized: {elapsed}", sw.Elapsed);

            return result;
        }

        throw new Exception("Could not load Hoh localization data");
    }
}
