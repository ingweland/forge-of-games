namespace Ingweland.Fog.App.Repositories;

/// <summary>
///     Disk-backed replacement for the IndexedDB stores the Blazor client uses to cache the Hoh core
///     data and localization blobs. One folder per data version, holding the core data or one file per
///     language: a new version replaces the old folder, and the files of one version stay together, as
///     in the Blazor client. Kept in the OS cache folder, which stays out of device backups; if the OS
///     clears it when storage runs low, the data is downloaded again.
/// </summary>
internal sealed class HohDataFileCache
{
    private readonly string _directory;

    public HohDataFileCache(string subdirectory)
    {
        _directory = Path.Combine(FileSystem.CacheDirectory, "hoh-data", subdirectory);
    }

    public async Task<byte[]?> TryReadAsync(string version, string name)
    {
        var path = GetPath(version, name);
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
    ///     Writes <paramref name="data" /> as <paramref name="name" /> of <paramref name="version" /> and
    ///     removes everything but that version's folder. A failure here is not fatal: the data is already
    ///     in memory and will simply be downloaded again next time.
    /// </summary>
    public async Task TryWriteAsync(string version, string name, byte[] data)
    {
        try
        {
            var versionDirectory = GetVersionDirectory(version);
            Directory.CreateDirectory(versionDirectory);

            // Other versions' folders, and the files of the earlier one-file-per-version layout.
            foreach (var stale in Directory.EnumerateFileSystemEntries(_directory)
                         .Where(x => !string.Equals(x, versionDirectory, StringComparison.OrdinalIgnoreCase)))
            {
                if (Directory.Exists(stale))
                {
                    Directory.Delete(stale, true);
                }
                else
                {
                    File.Delete(stale);
                }
            }

            await File.WriteAllBytesAsync(GetPath(version, name), data);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private string GetVersionDirectory(string version)
    {
        return Path.Combine(_directory, ToFileName(version));
    }

    private string GetPath(string version, string name)
    {
        return Path.Combine(GetVersionDirectory(version), $"{ToFileName(name)}.bin");
    }

    private static string ToFileName(string value)
    {
        return string.Concat(value.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
    }
}
