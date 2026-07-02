using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using Ingweland.Fog.Application.Client.Core.Localization;
using Ingweland.Fog.Application.Client.Web.Factories.Interfaces;
using Ingweland.Fog.Application.Client.Web.Services.Hoh.Abstractions;
using Ingweland.Fog.Dtos.Hoh.Battle;
using Ingweland.Fog.Models.Hoh.Enums;
using Microsoft.Extensions.Localization;

namespace Ingweland.Fog.Application.Client.Web.Factories;

public class BattleSearchRequestFactory(
    ICampaignUiService campaignUiService,
    IStringLocalizer<FogResource> localizer,
    IResourceLocalizationService resourceLocalizationService)
    : IBattleSearchRequestFactory
{
    private const string BattleTypeKey = "battleType";
    private const string CampaignRegionKey = "campaignRegion";
    private const string CampaignRegionEncounterKey = "campaignRegionEncounter";
    private const string DifficultyKey = "difficulty";
    private const string TreasureHuntDifficultyKey = "treasureHuntDifficulty";
    private const string TreasureHuntEncounterKey = "treasureHuntEncounter";
    private const string TreasureHuntStageKey = "treasureHuntStage";
    private const string HistoricBattleRegionKey = "historicBattleRegion";
    private const string HistoricBattleEncounterKey = "historicBattleEncounter";
    private const string TeslaStormRegionKey = "teslaStormRegion";
    private const string TeslaStormEncounterKey = "teslaStormEncounter";
    private const string BattleEventRegionKey = "battleEventRegion";
    private const string BattleEventEncounterKey = "battleEventEncounter";
    private const string UnitIdKey = "unitId";

    private static readonly BattleSearchRequest DefaultInfo = new();

    private readonly Dictionary<RegionId, string> _battleEventAbbreviations = new()
    {
        {RegionId.AncientEgyptDungeon, "Anubis"},
        {RegionId.ScyllaDungeon, "Scylla"},
    };

    private readonly Dictionary<RegionId, string> _historicBattlesAbbreviations = new()
    {
        {RegionId.SiegeOfOrleans, "SO"},
        {RegionId.SpartasLastStand, "SLS"},
        {RegionId.FallOfTroy, "FT"},
    };

    private readonly Dictionary<RegionId, string> _teslaAbbreviations = new()
    {
        {RegionId.TeslaStormBlue, "B"},
        {RegionId.TeslaStormGreen, "G"},
        {RegionId.TeslaStormRed, "R"},
        {RegionId.TeslaStormYellow, "Y"},
        {RegionId.TeslaStormPurple, "P"},
    };

    private readonly Dictionary<int, string> _treasureHuntAbbreviations = new()
    {
        {0, "RI"},
        {1, "RII"},
        {2, "AI"},
        {3, "AII"},
        {4, "VI"},
        {5, "VII"},
        {6, "MI"},
        {7, "MII"},
        {8, "GMI"},
        {9, "GMII"},
    };

    public bool TryCreate(string uri, out BattleSearchRequest request)
    {
        request = new BattleSearchRequest();
        if (string.IsNullOrWhiteSpace(uri))
        {
            return false;
        }

        var query = HttpUtility.ParseQueryString(new Uri(uri).Query);

        if (!query.HasKeys())
        {
            return false;
        }

        request = Create(query);
        return true;
    }

    public IReadOnlyDictionary<string, object?> CreateQueryParams(BattleSearchRequest request)
    {
        return new Dictionary<string, object?>
        {
            [BattleTypeKey] = request.BattleType.ToString(),
            [BattleEventRegionKey] = request.BattleEventRegion.ToString(),
            [BattleEventEncounterKey] = request.BattleEventEncounter.ToString(),
            [CampaignRegionKey] = request.CampaignRegion.ToString(),
            [CampaignRegionEncounterKey] = request.CampaignRegionEncounter,
            [DifficultyKey] = request.Difficulty.ToString(),
            [TreasureHuntDifficultyKey] = request.TreasureHuntDifficulty,
            [TreasureHuntEncounterKey] = request.TreasureHuntEncounter,
            [TreasureHuntStageKey] = request.TreasureHuntStage,
            [HistoricBattleRegionKey] = request.HistoricBattleRegion.ToString(),
            [HistoricBattleEncounterKey] = request.HistoricBattleEncounter,
            [TeslaStormRegionKey] = request.TeslaStormRegion.ToString(),
            [TeslaStormEncounterKey] = request.TeslaStormEncounter,
            [UnitIdKey] = request.UnitIds.ToHashSet().ToArray(),
        }.AsReadOnly();
    }

    public IReadOnlyDictionary<string, object?> CreateQueryParams(string battleDefinitionId, Difficulty difficulty,
        BattleType battleType, IEnumerable<string>? unitIds,
        IReadOnlyDictionary<(int difficulty, int stage), ReadOnlyDictionary<int, int>> treasureHuntEncounterMap)
    {
        var queryParams = new Dictionary<string, object?>();
        if (string.IsNullOrWhiteSpace(battleDefinitionId))
        {
            return queryParams;
        }

        queryParams.Add(BattleTypeKey, battleType.ToString());
        queryParams.Add(DifficultyKey, difficulty.ToString());
        const char delimiter = '_';
        var battleDefinitionIdParts = battleDefinitionId.Split(delimiter);
        if (battleType == BattleType.Campaign)
        {
            queryParams.Add(CampaignRegionKey, $"{battleDefinitionIdParts[0]}{delimiter}{battleDefinitionIdParts[1]}");
            queryParams.Add(CampaignRegionEncounterKey, battleDefinitionIdParts[2]);
        }

        if (battleType == BattleType.TreasureHunt)
        {
            var athDifficulty = int.Parse(battleDefinitionIdParts[1]);
            var athStage = int.Parse(battleDefinitionIdParts[2]);
            var athEncounter = int.Parse(battleDefinitionIdParts[3]);
            queryParams.Add(TreasureHuntDifficultyKey, athDifficulty);
            queryParams.Add(TreasureHuntStageKey, athStage);
            if (treasureHuntEncounterMap.TryGetValue((athDifficulty, athStage), out var map) &&
                map.TryGetValue(athEncounter, out var index))
            {
                queryParams.Add(TreasureHuntEncounterKey, index);
            }
        }

        if (battleType == BattleType.HistoricBattle)
        {
            queryParams.Add(HistoricBattleRegionKey, battleDefinitionIdParts[0]);
            queryParams.Add(HistoricBattleEncounterKey, battleDefinitionIdParts[1]);
        }

        if (battleType == BattleType.TeslaStorm)
        {
            queryParams.Add(TeslaStormRegionKey, battleDefinitionIdParts[0]);
            queryParams.Add(TeslaStormEncounterKey, battleDefinitionIdParts[1]);
        }

        if (battleType == BattleType.BattleEvent)
        {
            var battleEventEncounter = int.Parse(battleDefinitionIdParts[3]);
            queryParams.Add(BattleEventRegionKey, battleDefinitionIdParts[0]);
            queryParams.Add(BattleEventEncounterKey, battleEventEncounter);
        }

        if (unitIds != null)
        {
            queryParams.Add(UnitIdKey, unitIds.ToHashSet().ToArray());
        }

        return queryParams.AsReadOnly();
    }

    public async Task<string> CreateDefinitionTitleAsync(BattleSearchRequest request)
    {
        string details;
        switch (request.BattleType)
        {
            case BattleType.Campaign:
            {
                var continents = await campaignUiService.GetCampaignContinentsBasicDataAsync();
                details = $"{
                    continents.SelectMany(x => x.Regions).FirstOrDefault(x => x.Id == request.CampaignRegion)
                        ?.DisplayIndex}–{request.CampaignRegionEncounter}";
                break;
            }
            case BattleType.TreasureHunt:
            {
                details = $"{_treasureHuntAbbreviations[request.TreasureHuntDifficulty]}–{request.TreasureHuntStage + 1
                }–{request.TreasureHuntEncounter + 1}";
                break;
            }
            case BattleType.HistoricBattle:
                details = $"{_historicBattlesAbbreviations[request.HistoricBattleRegion]}–{
                    request.HistoricBattleEncounter}";
                break;
            case BattleType.TeslaStorm:
                details = $"{_teslaAbbreviations[request.TeslaStormRegion]}–{request.TeslaStormEncounter}";
                break;
            case BattleType.BattleEvent:
                details = $"{_battleEventAbbreviations[request.BattleEventRegion]}–{request.BattleEventEncounter
                }";
                break;
            default:
                details = string.Empty;
                break;
        }

        var sb = new StringBuilder();
        sb.Append(resourceLocalizationService.Localize(request.BattleType));

        if (request.Difficulty == Difficulty.Hard)
        {
            sb.Append('|');
            sb.Append(localizer[FogResource.Battle_Difficulty_Hard]);
        }

        if (!string.IsNullOrEmpty(details))
        {
            sb.Append(' ');
            sb.Append(details);
        }

        if (request.UnitIds.Count == 1)
        {
            sb.Append($" [1 {localizer[FogResource.Hoh_Hero]}]");
        }
        else if (request.UnitIds.Count > 1)
        {
            sb.Append($" [{request.UnitIds.Count} {localizer[FogResource.Hoh_Heroes]}]");
        }

        return sb.ToString();
    }

    public async Task<string> CreateDefinitionTitleAsync(string battleDefinitionId, BattleType battleType,
        Difficulty battleDifficulty,
        IReadOnlyDictionary<(int difficulty, int stage), ReadOnlyDictionary<int, int>> treasureHuntEncounterMap)
    {
        var query = CreateQueryParams(battleDefinitionId, battleDifficulty, battleType, null,
            treasureHuntEncounterMap);
        var nvc = new NameValueCollection();
        foreach (var kvp in query)
        {
            nvc.Add(kvp.Key, kvp.Value?.ToString());
        }

        var request = Create(nvc);
        return await CreateDefinitionTitleAsync(request);
    }

    private BattleSearchRequest Create(NameValueCollection query)
    {
        var battleTypeValue = query[BattleTypeKey];
        if (string.IsNullOrWhiteSpace(battleTypeValue) ||
            !Enum.TryParse<BattleType>(battleTypeValue, out var battleType))
        {
            battleType = DefaultInfo.BattleType;
        }

        var campaignRegionEncounterValue = query[CampaignRegionEncounterKey];
        if (string.IsNullOrWhiteSpace(campaignRegionEncounterValue) ||
            !int.TryParse(campaignRegionEncounterValue, out var campaignRegionEncounter))
        {
            campaignRegionEncounter = DefaultInfo.CampaignRegionEncounter;
        }

        var difficultyValue = query[DifficultyKey];
        if (string.IsNullOrWhiteSpace(difficultyValue) ||
            !Enum.TryParse<Difficulty>(difficultyValue, out var difficulty))
        {
            difficulty = DefaultInfo.Difficulty;
        }

        var campaignRegionValue = query[CampaignRegionKey];
        if (string.IsNullOrWhiteSpace(campaignRegionValue) ||
            !Enum.TryParse<RegionId>(campaignRegionValue, out var regionId))
        {
            regionId = DefaultInfo.CampaignRegion;
        }

        int.TryParse(query[TreasureHuntDifficultyKey], out var treasureHuntDifficulty);
        int.TryParse(query[TreasureHuntEncounterKey], out var treasureHuntEncounter);
        int.TryParse(query[TreasureHuntStageKey], out var treasureHuntStage);

        var historicBattleRegionValue = query[HistoricBattleRegionKey];
        if (string.IsNullOrWhiteSpace(historicBattleRegionValue) ||
            !Enum.TryParse<RegionId>(historicBattleRegionValue, out var historicBattleRegionId))
        {
            historicBattleRegionId = DefaultInfo.HistoricBattleRegion;
        }

        var historicBattleEncounterValue = query[HistoricBattleEncounterKey];
        if (string.IsNullOrWhiteSpace(historicBattleEncounterValue) ||
            !int.TryParse(historicBattleEncounterValue, out var historicBattleEncounter))
        {
            historicBattleEncounter = DefaultInfo.HistoricBattleEncounter;
        }

        var teslaStormRegionValue = query[TeslaStormRegionKey];
        if (string.IsNullOrWhiteSpace(teslaStormRegionValue) ||
            !Enum.TryParse<RegionId>(teslaStormRegionValue, out var teslaStormRegionId))
        {
            teslaStormRegionId = DefaultInfo.TeslaStormRegion;
        }

        var teslaStormEncounterValue = query[TeslaStormEncounterKey];
        if (string.IsNullOrWhiteSpace(teslaStormEncounterValue) ||
            !int.TryParse(teslaStormEncounterValue, out var teslaStormEncounter))
        {
            teslaStormEncounter = DefaultInfo.TeslaStormEncounter;
        }

        var battleEventRegionValue = query[BattleEventRegionKey];
        if (string.IsNullOrWhiteSpace(battleEventRegionValue) ||
            !Enum.TryParse<RegionId>(battleEventRegionValue, out var battleEventRegionId))
        {
            battleEventRegionId = DefaultInfo.BattleEventRegion;
        }

        var battleEventEncounterValue = query[BattleEventEncounterKey];
        if (string.IsNullOrWhiteSpace(battleEventEncounterValue) ||
            !int.TryParse(battleEventEncounterValue, out var battleEventEncounter))
        {
            battleEventEncounter = DefaultInfo.BattleEventEncounter;
        }

        return new BattleSearchRequest
        {
            BattleType = battleType,
            CampaignRegion = regionId,
            CampaignRegionEncounter = campaignRegionEncounter,
            Difficulty = difficulty,
            TreasureHuntDifficulty = treasureHuntDifficulty,
            TreasureHuntEncounter = treasureHuntEncounter,
            TreasureHuntStage = treasureHuntStage,
            HistoricBattleRegion = historicBattleRegionId,
            HistoricBattleEncounter = historicBattleEncounter,
            TeslaStormRegion = teslaStormRegionId,
            TeslaStormEncounter = teslaStormEncounter,
            UnitIds = query.GetValues(UnitIdKey) ?? [],
            BattleEventRegion = battleEventRegionId,
            BattleEventEncounter = battleEventEncounter,
        };
    }
}
