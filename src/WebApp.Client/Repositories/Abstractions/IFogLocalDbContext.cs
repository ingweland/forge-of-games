using CloudNimble.BlazorEssentials.IndexedDb;

namespace Ingweland.Fog.WebApp.Client.Repositories.Abstractions;

public interface IFogLocalDbContext
{
    public IndexedDbObjectStore HohCoreData { get; }
    public IndexedDbObjectStore HohLocalizationData { get; }
}
