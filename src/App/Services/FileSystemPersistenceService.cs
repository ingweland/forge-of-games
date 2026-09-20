using System.Text.Json;
using Ingweland.Fog.Application.Client.Web.Models;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Equipment;
using Ingweland.Fog.Models.Hoh.Enums;
using Ingweland.Fog.Shared.Helpers.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.App.Services;

/// <summary>
///     The part of WebApp.Client's PersistenceService the guide viewer needs: the community guides, which the
///     web keeps in local storage and this keeps as one protobuf file each, and the small key-value items it
///     stores next to them. The guides go in the cache folder like the rest of the downloaded data: they can be
///     fetched again, so they stay out of device backups.
///     Everything else still throws. The app has no cities or profiles of its own, and code that reaches for
///     them should fail loudly rather than silently lose data.
/// </summary>
internal sealed class FileSystemPersistenceService(
    IProtobufSerializer protobufSerializer,
    ILogger<FileSystemPersistenceService> logger) : IPersistenceService
{
    private static readonly string CommunityDirectory =
        Path.Combine(FileSystem.CacheDirectory, "city-strategies", "community");

    public ValueTask SaveCommunityCityStrategy(string strategyId, CityStrategy cityStrategy)
    {
        try
        {
            Directory.CreateDirectory(CommunityDirectory);
            protobufSerializer.SerializeToFile(cityStrategy, GetCommunityPath(strategyId));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // Not fatal: the guide is already in memory and is downloaded again next time.
            logger.LogWarning(e, "Could not cache the city guide {StrategyId}", strategyId);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    ///     Only the community guides are stored, so anything else is simply absent. Nothing in the app asks for
    ///     a strategy of its own, and "there is none" is the truthful answer if something ever does.
    /// </summary>
    public ValueTask<CityStrategy?> LoadCityStrategy(string strategyId, bool isCommunity = false)
    {
        var path = GetCommunityPath(strategyId);
        if (!isCommunity || !File.Exists(path))
        {
            return ValueTask.FromResult<CityStrategy?>(null);
        }

        try
        {
            return ValueTask.FromResult<CityStrategy?>(protobufSerializer.DeserializeFromFile<CityStrategy>(path));
        }
        catch (Exception e)
        {
            // A truncated or outdated file: drop it so the next open downloads the guide again.
            logger.LogWarning(e, "Could not read the cached city guide {StrategyId}", strategyId);
            TryDelete(path);
            return ValueTask.FromResult<CityStrategy?>(null);
        }
    }

    public ValueTask SetItemAsync<T>(string key, T value)
    {
        Preferences.Set(key, JsonSerializer.Serialize(value));
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveItemAsync(string key)
    {
        Preferences.Remove(key);
        return ValueTask.CompletedTask;
    }

    public ValueTask<T?> GetItemAsync<T>(string key)
    {
        var raw = Preferences.Get(key, string.Empty);
        if (string.IsNullOrEmpty(raw))
        {
            return ValueTask.FromResult<T?>(default);
        }

        try
        {
            return ValueTask.FromResult(JsonSerializer.Deserialize<T>(raw));
        }
        catch (JsonException e)
        {
            logger.LogWarning(e, "Could not read the stored item {Key}", key);
            return ValueTask.FromResult<T?>(default);
        }
    }

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

    // The share ids are opaque, so they get the same pass HohDataFileCache gives its names.
    private static string GetCommunityPath(string strategyId)
    {
        var name = string.Concat(strategyId.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        return Path.Combine(CommunityDirectory, $"{name}.bin");
    }

    private void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(e, "Could not delete the cached city guide at {Path}", path);
        }
    }
}
