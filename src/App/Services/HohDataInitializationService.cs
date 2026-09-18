using Ingweland.Fog.App.Services.Abstractions;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.App.Services;

/// <summary>
///     Loads the Hoh core data and localization blobs. Must complete before anything touches game
///     data: HohDataProviderBase.GetDataAsync returns its cached field and never loads lazily.
///     Mirrors WebApp.Client's HohDataInitializationService.
/// </summary>
public class HohDataInitializationService(
    IHohDataProvider hohDataProvider,
    IHohDataService hohDataService,
    IHohLocalizationDataProvider hohLocalizationDataProvider,
    ILogger<HohDataInitializationService> logger) : IHohDataInitializationService
{
    private Task? _initialization;

    public Task InitializeAsync()
    {
        return _initialization ??= InitializeCoreAsync();
    }

    private async Task InitializeCoreAsync()
    {
        var version = await hohDataService.GetHohCoreDataVersionAsync();
        await ((IDataProvider) hohDataProvider).InitializeAsync(version.Version);
        await ((IDataProvider) hohLocalizationDataProvider).InitializeAsync(version.Version);
        logger.LogDebug("Hoh data {version} initialized", version.Version);
    }
}
