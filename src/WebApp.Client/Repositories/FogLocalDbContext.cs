using CloudNimble.BlazorEssentials.IndexedDb;
using Ingweland.Fog.WebApp.Client.Repositories.Abstractions;
using Microsoft.JSInterop;

namespace Ingweland.Fog.WebApp.Client.Repositories;

public class FogLocalDbContext : IndexedDbDatabase, IFogLocalDbContext
{
    public FogLocalDbContext(IJSRuntime jsRuntime) : base(jsRuntime)
    {
        Name = "ForgeOfGames";
        Version = 1;
    }

    [ObjectStore(Name = "hoh-core-data", AutoIncrementKeys = false,
        KeyPath = Application.Client.Web.Data.Entities.HohCoreData.Keys.ID)]
    public IndexedDbObjectStore HohCoreData { get; set; } = null!;

    [ObjectStore(Name = "hoh-localization-data", AutoIncrementKeys = false,
        KeyPath = Application.Client.Web.Data.Entities.HohLocalizationData.Keys.ID)]
    public IndexedDbObjectStore HohLocalizationData { get; set; } = null!;
}
