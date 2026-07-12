using System.Globalization;
using Ingweland.Fog.Application.Server.Battle.Queries;
using Ingweland.Fog.Application.Server.Factories.Interfaces;
using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Application.Server.PlayerCity.Queries;
using Ingweland.Fog.Application.Server.Services.Queries;
using Ingweland.Fog.Application.Server.StatsHub.Queries;
using Ingweland.Fog.Application.Server.StatsHub.Queries.Tops;

namespace Ingweland.Fog.Application.Server.Factories;

public class CacheKeyFactory : ICacheKeyFactory
{
    public string Alliance(int allianceId)
    {
        return $"Alliance:{allianceId}";
    }

    public string HohCoreData => "HohCoreData";

    public string HohLocalizationData(string cultureCode)
    {
        return $"HohLocalizationData:{cultureCode}";
    }

    public string HeroAbilityFeatures(string cultureCode)
    {
        return $"HeroAbilityFeaturesQuery:{cultureCode}";
    }

    public string CreateKey<TRequest>(TRequest request) where TRequest : ICacheableRequest
    {
        return request switch
        {
            GetAllianceQuery q => Alliance(q.AllianceId),
            BattleSearchQuery q => $"BattleSearch:{q.BattleDefinitionId}:{q.BattleType}:{q.ResultStatus}:{
                string.Join("-", q.UnitIds)}",
            GetBattleQuery q => $"Battle:{q.Id}",
            GetBattleStatsQuery q => $"BattleStats:{q.Id}:{CultureInfo.CurrentCulture.Name}",
            GetUnitBattlesQuery q => $"UnitBattles:{q.UnitId}:{q.BattleType}:{CultureInfo.CurrentCulture.Name}",
            CityInspirationsSearchQuery q => $"CityInspirationsSearch:{q.Request.CityId}:{q.Request.AgeId}:{
                q.Request.SearchPreference}:{q.Request.ProductionMetric}:{q.Request.AllowPremiumHomeBuildings}:{
                    q.Request.AllowPremiumFarmBuildings}:{q.Request.AllowPremiumCultureBuildings}:{
                        q.Request.OpenedExpansionsHash}:{q.Request.TotalArea}",
            GetPlayerCityFromSnapshotQuery q => $"PlayerCityFromSnapshot:{q.SnapshotId}",
            GetPlayerCityQuery q => $"PlayerCity:{q.PlayerId}:{q.Date}",
            GetPlayerEventCityQuery q => $"PlayerEventCity:{q.PlayerId}",
            GetTopPlayersQuery q => $"TopPlayers:{q.WorldId}",
            GetTopAlliancesQuery q => $"TopAlliances:{q.WorldId}",
            GetPlayerBattlesQuery q => $"PlayerBattles:{q.PlayerId}:{q.StartIndex}:{q.Count}",
            GetPlayerProfileQuery q => $"PlayerProfile:{q.PlayerId}",
            GetPlayerQuery q => Player(q.PlayerId),
            GetTopHeroesQuery q => $"TopHeroes:{q.Mode}:{q.AgeId}:{q.FromLevel}:{q.ToLevel}",
            GetEquipmentInsightsQuery q => $"EquipmentInsights:{q.UnitId}",
            GetRelicInsightsQuery q => $"RelicInsights:{q.UnitId}",
            GetAllianceAthRankingsQuery q => $"AllianceAthRankings:{q.AllianceId}",
            GetAllianceRankingsQuery q => $"AllianceRankings:{q.AllianceId}",
            GetPlayerPvpRankingsQuery q => $"PlayerPvpRankings:{q.PlayerId}",
            GetPlayerRankingsQuery q => $"PlayerRankings:{q.PlayerId}",
            GetEventsQuery q => $"InGameEvents:{q.WorldId}:{q.EventDefinitionId}",
            GetCurrentInGameEventQuery q => $"CurrentInGameEvent:{q.WorldId}:{q.EventDefinitionId}",
            GetAlliancesAthRankingsQuery q =>
                $"AlliancesAthRankings:{q.WorldId}:{q.League}:{q.StartIndex}:{q.PageSize}",
            GetEventCityRankingsQuery q => $"EventCityRankingsQuery:{q.WorldId}",
            GetAnnualBudgetQuery q => $"AnnualBudget:{q.Year}",
            GetPlayerCityPropertiesQuery q => $"PlayerCityProperties:{q.PlayerId}",
            GetCommunityCityStrategiesQuery q => "CommunityCityStrategies",
            GetCommunityCityGuidesQuery q => "CommunityCityGuides",
            GetCommunityCityGuideQuery q => $"CommunityCityGuide:{q.Id}",
            GetAlliancesWithPaginationQuery q => $"Alliances:{q.WorldId}:{q.StartIndex}:{q.PageSize}:{q.Name}",
            GetPlayersWithPaginationQuery q => $"Players:{q.WorldId}:{q.StartIndex}:{q.PageSize}:{q.Name}",
            GetPlayerAthRankingsQuery q => $"PlayerAthRankings:{q.PlayerId}",
            GetSharedResourceQuery q => $"SharedResource:{q.ShareId}",
            GetHeroAbilityFeaturesQuery q => HeroAbilityFeatures(CultureInfo.CurrentCulture.Name),
            GetHohCoreDataVersionQuery q => "HohCoreDataVersion",
            GetAllianceWoaRankingsQuery q => $"AllianceWoaRankings:{q.AllianceId}",
            GetWoaPlayerStatsQuery q => $"WoaPlayerStats:{q.PlayerId}",
            _ => typeof(TRequest).FullName ?? Guid.NewGuid().ToString(),
        };
    }

    public string Player(int playerId)
    {
        return $"Player:{playerId}";
    }
}
