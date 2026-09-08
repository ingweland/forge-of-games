using Ingweland.Fog.Functions.Services;
using Microsoft.Azure.Functions.Worker;

namespace Ingweland.Fog.Functions.Functions;

public class HohCoreDataFetcherTrigger(IHohCoreDataFetcher coreDataFetcher)
{
    [Function(nameof(HohCoreDataFetcherTrigger))]
    public async Task Run([TimerTrigger("0 0 0 1 1 1", RunOnStartup = false)] TimerInfo myTimer)
    {
        await coreDataFetcher.RunAsync();
    }
}
