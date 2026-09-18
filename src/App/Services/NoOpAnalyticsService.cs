using Ingweland.Fog.Application.Client.Web.Analytics.Interfaces;

namespace Ingweland.Fog.App.Services;

/// <summary>
///     The web's GoogleAnalyticsService reports through gtag via JS interop, which does not exist here.
///     Shared UI services (e.g. FogSharingUiService) fire analytics events on every call, so this
///     swallows them until the app gets its own analytics.
/// </summary>
internal sealed class NoOpAnalyticsService : IAnalyticsService
{
    public Task TrackEvent(string eventName, IReadOnlyDictionary<string, object> eventParams)
    {
        return Task.CompletedTask;
    }
}
