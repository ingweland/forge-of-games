using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.App.Repositories;

/// <summary>
///     Downloaded assets on disk, one file per URL, never refreshed: suits URLs whose content never changes in
///     place. Kept in the OS cache folder, so they survive restarts; if the OS clears it when storage runs low,
///     they are downloaded again. A file is written under a temp name and then renamed, so a reader never sees
///     half of one. Disk errors are logged and treated as misses.
/// </summary>
internal sealed class AssetFileCache
{
    private readonly string _directory;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly ILogger<AssetFileCache> _logger;

    public AssetFileCache(ILogger<AssetFileCache> logger)
    {
        _logger = logger;
        _directory = Path.Combine(FileSystem.CacheDirectory, "assets");
        _logger.LogInformation("Asset cache folder: {directory}", _directory);
    }

    /// <summary>
    ///     Makes requests for the same URL wait for each other, so an icon shown several times is downloaded
    ///     once. There is one lock per distinct URL, and they are never removed.
    /// </summary>
    public async Task<IDisposable> LockAsync(string url, CancellationToken cancellationToken)
    {
        var semaphore = _locks.GetOrAdd(url, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new Releaser(semaphore);
    }

    public async Task<byte[]?> TryReadAsync(string url, CancellationToken cancellationToken)
    {
        var path = GetPath(url);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            return await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            _logger.LogDebug(e, "Could not read the cached copy of {url}", url);
            return null;
        }
    }

    public async Task TryWriteAsync(string url, byte[] content)
    {
        var path = GetPath(url);
        // A unique temp name in the same folder, so the rename replaces the file in one step.
        var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        try
        {
            Directory.CreateDirectory(_directory);
            await File.WriteAllBytesAsync(tempPath, content).ConfigureAwait(false);
            File.Move(tempPath, path, true);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            _logger.LogDebug(e, "Could not cache {url}", url);
            TryDelete(tempPath);
        }
    }

    // A hash of the URL, keeping its extension so the folder is easy to inspect.
    private string GetPath(string url)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url)));
        return Path.Combine(_directory, hash + Path.GetExtension(new Uri(url).AbsolutePath));
    }

    private void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            _logger.LogDebug(e, "Could not delete {path}", path);
        }
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose()
        {
            semaphore.Release();
        }
    }
}
