using System.Net;
using Ingweland.Fog.App.Repositories;

namespace Ingweland.Fog.App.Services;

/// <summary>
///     Serves an HttpClient's downloads from <see cref="AssetFileCache" />: the first successful GET of a URL is
///     stored, and every later one is answered from disk without a request, also after a restart. Only for
///     clients that load assets whose URL changes when their content does.
/// </summary>
internal sealed class AssetCacheHandler(AssetFileCache cache) : DelegatingHandler
{
    /// <summary>The named HttpClient for images shown in the UI.</summary>
    public const string HTTP_CLIENT_NAME = "Assets";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get || request.RequestUri is not {IsAbsoluteUri: true} uri)
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        var url = uri.AbsoluteUri;
        using var urlLock = await cache.LockAsync(url, cancellationToken).ConfigureAwait(false);

        var content = await cache.TryReadAsync(url, cancellationToken).ConfigureAwait(false);
        if (content == null)
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return response;
            }

            using (response)
            {
                content = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            }

            await cache.TryWriteAsync(url, content).ConfigureAwait(false);
        }

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(content),
            RequestMessage = request,
        };
    }
}
