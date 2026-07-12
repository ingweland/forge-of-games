using Ingweland.Fog.Models.Fog.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ingweland.Fog.Application.Server.Interfaces;

public interface IFogDbContext
{
    DbSet<AllianceAthRanking> AllianceAthRankings { get; set; }
    DbSet<AllianceRanking> AllianceRankings { get; set; }
    DbSet<Alliance> Alliances { get; set; }
    DbSet<AllianceWoaRanking> AllianceWoaRankings { get; set; }
    DbSet<AnnualBudget> AnnualBudgets { get; set; }
    DbSet<BattleSummaryEntity> Battles { get; set; }
    DbSet<BattleStatsEntity> BattleStats { get; set; }
    DbSet<BattleTimelineEntity> BattleTimelines { get; set; }
    DbSet<BattleUnitEntity> BattleUnits { get; set; }
    public DbSet<CommunityCityGuideEntity> CommunityCityGuides { get; set; }
    public DbSet<CommunityCityStrategyEntity> CommunityCityStrategies { get; set; }
    DbSet<EquipmentInsightsEntity> EquipmentInsights { get; set; }
    DbSet<EventCityFetchState> EventCityFetchStates { get; set; }
    DbSet<EventCitySnapshot> EventCitySnapshots { get; set; }
    DbSet<EventCityStrategy> EventCityStrategies { get; set; }
    DbSet<EventCityWonderRanking> EventCityWonderRankings { get; set; }
    public DbSet<HeroAbilityFeaturesEntity> HeroAbilityFeatures { get; set; }
    DbSet<InGameEventEntity> InGameEvents { get; set; }
    DbSet<PlayerAthRanking> PlayerAthRankings { get; set; }
    DbSet<PlayerCitySnapshot> PlayerCitySnapshots { get; set; }
    DbSet<PlayerHeroEntity> PlayerHeroes { get; set; }
    DbSet<PlayerRanking> PlayerRankings { get; set; }
    DbSet<Player> Players { get; set; }
    DbSet<ProfileSquadDataEntity> ProfileSquadDataItems { get; set; }
    DbSet<ProfileSquadEntity> ProfileSquads { get; set; }
    DbSet<PvpBattle> PvpBattles { get; set; }
    DbSet<PvpRanking2> PvpRankings { get; set; }
    DbSet<RelicInsightsEntity> RelicInsights { get; set; }
    DbSet<SharedSubmissionIdEntity> SharedSubmissionIds { get; set; }
    DbSet<TopHeroInsightsEntity> TopHeroInsights { get; set; }
    DbSet<WoaPlayerStatsEntity> WoaPlayerStats { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
