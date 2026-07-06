using System.Collections.ObjectModel;
using Ingweland.Fog.Application.Server.Settings;
using Ingweland.Fog.HohCoreDataParserSdk;
using Ingweland.Fog.Infrastructure.Repositories.Abstractions;
using Ingweland.Fog.InnSdk.Hoh.Abstractions;
using Ingweland.Fog.InnSdk.Hoh.Authentication.Models;
using Ingweland.Fog.Shared.Localization;
using Ingweland.Fog.Shared.Utils;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Functions.Services;

public interface IHohCoreDataFetcher
{
    Task RunAsync();
}

public class HohCoreDataFetcher(
    IInnSdkClient sdkClient,
    IHohRawCoreDataAzureContainerClient rawDataContainerClient,
    IHohCoreDataAzureContainerClient dataContainerClient,
    GameDesignDataParser gameDesignDataParser,
    LocalizationParser localizationParser,
    IDynamicConfigurationProvider dynamicConfigurationProvider,
    IHeroAttributeFeaturesParser attributeFeaturesParser,
    ILogger<HohCoreDataFetcher> logger)
    : IHohCoreDataFetcher
{
    private const string DELIMITER = "/";

    private static readonly HashSet<string> StartupFileNames =
    [
        "startup_china-maya.bin", "startup_maya-egypt.bin", "startup_egypt-vikings.bin",
        "startup_ancientEgypt-arabia-vikings.bin", "startup_ithaka.bin",
    ];

    private readonly GameWorldConfig _betaWorldConfig = new("zz", 1, "beta");

    public async Task RunAsync()
    {
        var data = await DownloadAsync();

        var versionDir = $"{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}";
        var gd = gameDesignDataParser.Parse(data.GameDesignData, await LoadStartupFilesAsync());
        var gdFilename = $"{versionDir}{DELIMITER}data.bin";
        await SaveDataAsync(gdFilename, gd);

        var localizationData = localizationParser.Parse(data.LocalizationData);
        foreach (var kvp in localizationData)
        {
            var fileName = $"{versionDir}{DELIMITER}{kvp.Key}";
            await SaveDataAsync(fileName, kvp.Value);
        }

        dynamicConfigurationProvider.UpdateValue(
            $"{ResourceSettings.CONFIGURATION_PROPERTY_NAME}:{nameof(ResourceSettings.HohCoreDataVersion)}",
            versionDir);

        await attributeFeaturesParser.RunAsync();

        await PerformDiffCheck();

        logger.LogInformation("""

                              ===================================
                              ║
                              ║   Version: {version}
                              ║
                              ===================================

                              """, versionDir);
    }

    private async Task PerformDiffCheck()
    {
        logger.LogInformation("\r\n==============================");
        logger.LogInformation("Performing diff check...");
        var dirs = new List<string>();
        await foreach (var item in dataContainerClient.Client.GetBlobsByHierarchyAsync(delimiter: DELIMITER))
        {
            if (item.IsPrefix)
            {
                dirs.Add(item.Prefix);
            }
        }

        var latestDirs = dirs.OrderByDescending(x => x).Take(2).ToList();
        if (latestDirs.Count < 2)
        {
            logger.LogInformation("Nothing to compare.");
        }

        logger.LogInformation("Dirs to compare: [{d1}, {d2}]", latestDirs[0], latestDirs[1]);
        var fileName1 = $"{latestDirs[0]}data.bin";
        var data1 = await GetDataBlobContentAsync(fileName1);
        if (data1 == null)
        {
            logger.LogWarning("Core data file {filename} not found.", fileName1);
        }

        var fileName2 = $"{latestDirs[1]}data.bin";
        var data2 = await GetDataBlobContentAsync(fileName2);
        if (data2 == null)
        {
            logger.LogWarning("Core data file {filename} not found.", fileName1);
        }

        var areDifferent = ByteArrayUtils.AreBytesEqual(data1, data2);
        logger.LogInformation("Core data files are equal: {b}", areDifferent);

        foreach (var l in HohSupportedCultures.AllCultures)
        {
            fileName1 = $"{latestDirs[0]}loca_parsed_{l}.bin";
            data1 = await GetDataBlobContentAsync(fileName1);
            if (data1 == null)
            {
                logger.LogWarning("Localization file {filename} not found.", fileName1);
            }

            fileName2 = $"{latestDirs[1]}loca_parsed_{l}.bin";
            data2 = await GetDataBlobContentAsync(fileName2);
            if (data2 == null)
            {
                logger.LogWarning("Localization file {filename} not found.", fileName2);
            }

            areDifferent = ByteArrayUtils.AreBytesEqual(data1, data2);
            logger.LogInformation("Localization files for locale {l} are equal: {b}", l, areDifferent);
        }

        logger.LogInformation("==============================");
    }

    private async Task<byte[]?> GetDataBlobContentAsync(string filename)
    {
        var blob = dataContainerClient.Client.GetBlobClient(filename);
        if (!await blob.ExistsAsync())
        {
            return null;
        }

        return dataContainerClient.Client.GetBlobClient(filename).DownloadContentAsync().Result.Value.Content.ToArray();
    }

    private async Task<IReadOnlyCollection<byte[]>> LoadStartupFilesAsync()
    {
        var result = new List<byte[]>();
        foreach (var filename in StartupFileNames)
        {
            var blobClient = rawDataContainerClient.Client.GetBlobClient(filename);
            var blobContent = await blobClient.DownloadContentAsync();
            var bytes = blobContent.Value.Content.ToArray();
            result.Add(bytes);
        }

        return result;
    }

    private async Task<(byte[] GameDesignData, IReadOnlyDictionary<string, byte[]> LocalizationData)> DownloadAsync()
    {
        logger.LogInformation("Downloading localization data.");
        var localeItems = await DownloadLocales();
        await DownloadJsonLocale();
        logger.LogInformation("Localization data downloaded.");

        logger.LogInformation("Downloading game design data.");
        var filename = $"gamedesign_{DateTime.Today:dd.MM.yy}.bin";
        var gd = await DownloadProtobufGamedesign(filename);
        filename = $"gamedesign_{DateTime.Today:dd.MM.yy}.json";
        await DownloadJsonGamedesign(filename);
        logger.LogInformation("Game design data downloaded.");

        logger.LogInformation("Downloading startup data");
        filename = $"startup_{DateTime.Today:dd.MM.yy}.bin";
        await DownloadProtobufStartup(filename);
        filename = $"startup_{DateTime.Today:dd.MM.yy}.json";
        await DownloadJsonStartup(filename);
        logger.LogInformation("Startup data downloaded.");

        return (gd, localeItems);
    }

    private async Task DownloadJsonLocale()
    {
        var filename = "loca_en-DK.json";
        var data = await sdkClient.StaticDataService.GetLocalizationJsonAsync(_betaWorldConfig, "en_DK");
        await SaveRawDataAsync(filename, data);
    }

    private async Task<ReadOnlyDictionary<string, byte[]>> DownloadLocales()
    {
        var locales = HohSupportedCultures.AllCultures.ToDictionary(c => c, c => c.Replace('-', '_'));
        var results = new Dictionary<string, byte[]>();
        foreach (var kvp in locales)
        {
            var filename = $"loca_{kvp.Key}.bin";
            var data = await sdkClient.StaticDataService.GetLocalizationProtobufAsync(_betaWorldConfig, kvp.Value);
            await SaveRawDataAsync(filename, data);
            results.Add(kvp.Key, data);
        }

        return results.AsReadOnly();
    }

    private async Task<byte[]> DownloadProtobufGamedesign(string filename)
    {
        var data = await sdkClient.StaticDataService.GetGameDesignProtobufAsync(_betaWorldConfig);
        await SaveRawDataAsync(filename, data);
        return data;
    }

    private async Task DownloadJsonGamedesign(string filename)
    {
        var data = await sdkClient.StaticDataService.GetGameDesignJsonAsync(_betaWorldConfig);
        await SaveRawDataAsync(filename, data);
    }

    private async Task DownloadProtobufStartup(string filename)
    {
        var data = await sdkClient.StaticDataService.GetStartupRawDataAsync(_betaWorldConfig);
        await SaveRawDataAsync(filename, data);
    }

    private async Task SaveRawDataAsync(string filename, byte[] data)
    {
        var blobClient = rawDataContainerClient.Client.GetBlobClient(filename);
        await blobClient.UploadAsync(new BinaryData(data), true);
    }

    private async Task SaveDataAsync(string filename, byte[] data)
    {
        var blobClient = dataContainerClient.Client.GetBlobClient(filename);
        await blobClient.UploadAsync(new BinaryData(data), false);
    }

    private async Task SaveRawDataAsync(string filename, string data)
    {
        var blobClient = rawDataContainerClient.Client.GetBlobClient(filename);
        await blobClient.UploadAsync(new BinaryData(data), true);
    }

    private async Task DownloadJsonStartup(string filename)
    {
        var data = await sdkClient.StaticDataService.GetStartupJsonAsync(_betaWorldConfig);
        await SaveRawDataAsync(filename, data);
    }
}
