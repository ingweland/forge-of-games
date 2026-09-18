using Ingweland.Fog.Application.Core.Repository.Abstractions;
using Microsoft.Extensions.Logging;

namespace Ingweland.Fog.Application.Client.Web.Repositories.Abstractions;

public abstract class HohDataProviderBase<TData>(
    ILogger<HohDataProviderBase<TData>> logger) : IDataProvider
{
    private TData _cachedData;
    private Task<TData>? _loadingTask;

    protected ILogger<HohDataProviderBase<TData>> Logger { get; } = logger;
    public Guid Version { get; } = Guid.NewGuid();

    public async Task InitializeAsync(string version)
    {
        _cachedData = await (_loadingTask ??= LoadAsync(version));
    }

    public Task<TData> GetDataAsync()
    {
        return Task.FromResult(_cachedData);
    }

    public TData GetData()
    {
        return _cachedData;
    }

    protected abstract Task<TData> LoadAsync(string version);
}
