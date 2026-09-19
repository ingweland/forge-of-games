using System.Globalization;
using Ingweland.Fog.App.Services.Abstractions;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Application.Core.Interfaces;
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
    IHohDataCache hohDataCache,
    ILogger<HohDataInitializationService> logger) : IHohDataInitializationService
{
    private string? _cultureCode;
    private Task? _initialization;

    public Task InitializeAsync()
    {
        // The game's texts are per language. After a language change in the side menu this loads them in the new
        // language and drops the data built from the old ones, as the website's page reload does.
        var cultureCode = CultureInfo.CurrentCulture.Name;
        if (_initialization != null && cultureCode == _cultureCode)
        {
            return _initialization;
        }

        if (_initialization != null)
        {
            hohDataCache.Clear();
        }

        _cultureCode = cultureCode;
        return _initialization = InitializeCoreAsync();
    }

    private async Task InitializeCoreAsync()
    {
        var version = await hohDataService.GetHohCoreDataVersionAsync();
        await ((IDataProvider) hohDataProvider).InitializeAsync(version.Version);
        await ((IDataProvider) hohLocalizationDataProvider).InitializeAsync(version.Version);
        logger.LogDebug("Hoh data {version} initialized", version.Version);
    }
}
