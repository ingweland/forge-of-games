namespace Ingweland.Fog.App.Repositories;

/// <summary>
///     Disk-backed replacement for the IndexedDB stores the Blazor client uses to cache the Hoh core
///     data and localization blobs. One file per version; stale versions are pruned on write. Kept in the
///     OS cache folder, which stays out of device backups; if the OS clears it when storage runs low, the
///     data is downloaded again.
/// </summary>
internal sealed class HohDataFileCache
{
    private readonly string _directory;

    public HohDataFileCache(string subdirectory)
    {
        _directory = Path.Combine(FileSystem.CacheDirectory, "hoh-data", subdirectory);
    }

    public async Task<byte[]?> TryReadAsync(string id)
    {
        var path = GetPath(id);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            return await File.ReadAllBytesAsync(path);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    /// <summary>
    ///     Writes <paramref name="data" /> for <paramref name="id" /> and removes every other cached
    ///     file. A failure here is not fatal: the data is already in memory and will simply be
    ///     downloaded again next time.
    /// </summary>
    public async Task TryWriteAsync(string id, byte[] data)
    {
        try
        {
            Directory.CreateDirectory(_directory);
            var path = GetPath(id);

            foreach (var stale in Directory.EnumerateFiles(_directory, "*.bin")
                         .Where(x => !string.Equals(x, path, StringComparison.OrdinalIgnoreCase)))
            {
                File.Delete(stale);
            }

            await File.WriteAllBytesAsync(path, data);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private string GetPath(string id)
    {
        var fileName = string.Concat(id.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        return Path.Combine(_directory, $"{fileName}.bin");
    }
}
