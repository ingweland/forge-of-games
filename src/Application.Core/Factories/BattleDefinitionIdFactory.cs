using System.Text;
using Ingweland.Fog.Application.Core.Factories.Interfaces;
using Ingweland.Fog.Application.Core.Helpers;
using Ingweland.Fog.Dtos.Hoh.Battle;
using Ingweland.Fog.Models.Hoh.Enums;

namespace Ingweland.Fog.Application.Core.Factories;

public class BattleDefinitionIdFactory : IBattleDefinitionIdFactory
{
    public string Create(BattleSearchRequest request)
    {
        var sb = new StringBuilder();

        return request.BattleType switch
        {
            BattleType.Campaign => CreateCampaignId(request, sb),
            BattleType.TreasureHunt => CreateTreasureHuntId(request, sb),
            BattleType.HistoricBattle => CreateHistoricBattleId(request, sb),
            BattleType.TeslaStorm => CreateTeslaStormId(request, sb),
            BattleType.BattleEvent => CreateBattleEventId(request, sb),
            BattleType.Pvp => "pvp",
            _ => sb.ToString(),
        };
    }

    private string CreateCampaignId(BattleSearchRequest request, StringBuilder sb)
    {
        sb.Append(request.CampaignRegion)
            .Append('_')
            .Append(request.CampaignRegionEncounter);

        if (request.Difficulty == Difficulty.Hard)
        {
            sb.Append("_Hard");
        }

        return sb.ToString();
    }

    private string CreateTeslaStormId(BattleSearchRequest request, StringBuilder sb)
    {
        sb.Append(request.TeslaStormRegion)
            .Append('_')
            .Append(request.TeslaStormEncounter);

        return sb.ToString();
    }

    private string CreateBattleEventId(BattleSearchRequest request, StringBuilder sb)
    {
        sb.Append(request.BattleEventRegion);
        if (request.BattleType == BattleType.BattleEvent)
        {
            sb.Append("_1");
        }

        sb.Append('_')
            .Append("Encounter")
            .Append('_')
            .Append(request.BattleEventEncounter);
        return sb.ToString();
    }

    private string CreateHistoricBattleId(BattleSearchRequest request, StringBuilder sb)
    {
        sb.Append(request.HistoricBattleRegion)
            .Append('_')
            .Append(request.HistoricBattleEncounter);

        return sb.ToString();
    }

    private string CreateTreasureHuntId(BattleSearchRequest request, StringBuilder sb)
    {
        if (request.TreasureHuntEncounter < 0)
        {
            return string.Empty;
        }

        var stage = TreasureHuntBattleEncounters.GetStage(request.TreasureHuntDifficulty, request.TreasureHuntStage);
        if (stage.Count == 0)
        {
            return string.Empty;
        }

        var encounters = stage.Select(x => int.Parse(x[(x.LastIndexOf('_') + 1)..])).Order().ToList();
        if (encounters.Count < request.TreasureHuntEncounter)
        {
            return string.Empty;
        }

        return sb.Append("Encounter")
            .Append('_')
            .Append(request.TreasureHuntDifficulty)
            .Append('_')
            .Append(request.TreasureHuntStage)
            .Append('_')
            .Append(encounters[request.TreasureHuntEncounter])
            .ToString();
    }
}
