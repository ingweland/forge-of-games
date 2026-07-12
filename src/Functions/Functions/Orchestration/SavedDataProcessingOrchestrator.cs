using System.Collections.ObjectModel;
using Ingweland.Fog.Functions.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace Ingweland.Fog.Functions.Functions.Orchestration;

public class SavedDataProcessingOrchestrator : SubOrchestratorBase
{
    protected override IReadOnlyDictionary<string, ActivityConfiguration> Activities =>
        new ReadOnlyDictionary<string, ActivityConfiguration>(new Dictionary<string, ActivityConfiguration>
        {
            {nameof(PlayerDataProcessor), new ActivityConfiguration(12)},
            {nameof(AllianceDataProcessor), new ActivityConfiguration(12)},
            {nameof(BattlesProcessor), new ActivityConfiguration(1)},
            {nameof(WoaPlayerStatsProcessor), new ActivityConfiguration(12)},
            {nameof(PlayerHeroesProcessor), new ActivityConfiguration(12)},
        });

    [Function(nameof(SavedDataProcessingOrchestrator))]
    public async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        await DoRunOrchestrator(context);
    }
}
