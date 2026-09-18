using Ingweland.Fog.Application.Client.Web.EquipmentConfigurator.Abstractions;
using Ingweland.Fog.Application.Client.Web.Models;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Equipment;
using Ingweland.Fog.Models.Hoh.Entities.Relics;
using Ingweland.Fog.Models.Hoh.Enums;
using Microsoft.AspNetCore.Components;

namespace Ingweland.Fog.App.Services;

// The city viewer never persists anything: CityPlanner takes IPersistenceService in its constructor
// but the read-only path never calls it. These throw rather than no-op so that any future code that
// does reach for persistence fails loudly instead of silently losing data.
// Replace with a real Preferences/file-backed implementation when the guide feature needs saving.
internal sealed class NotSupportedPersistenceService : IPersistenceService
{
    public ValueTask SaveCity(HohCity city)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask SaveCityStrategy(CityStrategy cityStrategy)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask SaveCityInspirationsRequestAsync(CityInspirationsSearchFormRequest request)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<CityInspirationsSearchFormRequest?> GetCityInspirationsRequestAsync()
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask SaveTopHeroesRequestAsync(TopHeroesSearchFormRequest request)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<TopHeroesSearchFormRequest?> GetTopHeroesRequestAsync()
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<bool> DeleteCity(string cityId)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<bool> DeleteCityStrategy(string strategyId)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<HohCity?> LoadCity(string cityId)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<CityStrategy?> LoadCityStrategy(string strategyId, bool isCommunity = false)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<IReadOnlyCollection<HohCityBasicData>> GetCities()
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<IReadOnlyCollection<HohCityBasicData>> GetCityStrategies()
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask SaveCommandCenterProfile(BasicCommandCenterProfile commandCenterProfile)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask SaveEquipment(IReadOnlyCollection<EquipmentItem> equipment)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<bool> DeleteProfile(string profileId)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<BasicCommandCenterProfile?> LoadProfile(string profileId)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<IReadOnlyCollection<BasicCommandCenterProfile>> GetProfilesAsync()
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<HeroProfileIdentifier?> GetHeroProfileAsync(string heroId)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<IReadOnlyCollection<EquipmentItem>> GetEquipmentAsync()
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask SaveHeroProfileAsync(HeroProfileIdentifier profile)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<UiSettings> GetUiSettingsAsync()
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask SaveUiSettingsAsync(UiSettings settings)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask SaveTempCities(IEnumerable<HohCity> cities)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<IReadOnlyCollection<HohCity>> GetTempCities()
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask SaveCityBackup(HohCityBackup cityBackup)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask SaveCommandCenterProfileBackup(CommandCenterProfileBackup backup)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask SaveOpenTechnologies(CityId cityId, IReadOnlyCollection<string> openTechnologies)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<IReadOnlyCollection<string>> GetOpenTechnologies(CityId cityId)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask SaveCommunityCityStrategy(string strategyId, CityStrategy cityStrategy)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask SetItemAsync<T>(string key, T value)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask RemoveItemAsync(string key)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<T?> GetItemAsync<T>(string key)
    {
        throw new NotSupportedException(NotSupported.Message);
    }
}

internal sealed class NotSupportedJsInteropService : IJSInteropService
{
    public ValueTask ResetScrollPositionAsync()
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask ShowLoadingIndicatorAsync()
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask ScrollTo(ElementReference target, int position, bool smooth = false)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask ScrollToBottomAsync(ElementReference target, bool smooth = false)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask SendToGtag(string command, string target, IReadOnlyDictionary<string, object> parameters)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask SaveFileAsync(string fileName, string contentType, object content)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask OpenPrivacySettingsAsync()
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<bool> IsMobileAsync()
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask OpenUrlAsync(string url, string target)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask HideLoadingIndicatorAsync()
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<bool> CopyToClipboardAsync(string payload)
    {
        throw new NotSupportedException(NotSupported.Message);
    }
}

internal sealed class NotSupportedEquipmentProfilePersistenceService : IEquipmentProfilePersistenceService
{
    public Task UpsertProfileAsync(string? profileId, string? profileName,
        IReadOnlyCollection<HeroProfileIdentifier> heroes, IReadOnlyCollection<RelicItem> relics,
        IReadOnlyCollection<EquipmentItem> equipment,
        BarracksProfile barracksProfile)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public Task SaveAsync(EquipmentProfile profile)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public Task<EquipmentProfile?> GetAsync(string profileId)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask DeleteAsync(string profileId)
    {
        throw new NotSupportedException(NotSupported.Message);
    }

    public ValueTask<IReadOnlyCollection<EquipmentProfileBasicData>> GetProfiles()
    {
        throw new NotSupportedException(NotSupported.Message);
    }
}

internal static class NotSupported
{
    public const string Message =
        "This service is not implemented in the MAUI app yet; the city viewer does not need it.";
}
