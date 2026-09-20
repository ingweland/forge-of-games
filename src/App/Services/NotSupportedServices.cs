using Ingweland.Fog.Application.Client.Web.EquipmentConfigurator.Abstractions;
using Ingweland.Fog.Application.Client.Web.Models;
using Ingweland.Fog.Application.Client.Web.Services.Abstractions;
using Ingweland.Fog.Models.Fog.Entities;
using Ingweland.Fog.Models.Hoh.Entities.Equipment;
using Ingweland.Fog.Models.Hoh.Entities.Relics;
using Microsoft.AspNetCore.Components;

namespace Ingweland.Fog.App.Services;

// Two services the client application layer asks for that the app has no counterpart for: the browser
// interop the web uses for scrolling, downloads and the clipboard, and the equipment profile storage
// behind a feature the app does not show. These throw rather than no-op so that any code reaching for
// them fails loudly instead of silently doing nothing.
// City guides and the items stored with them live in FileSystemPersistenceService.
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
        "This service is not implemented in the MAUI app; nothing it shows needs it.";
}
