using Ingweland.Fog.Application.Server.Interfaces;
using Ingweland.Fog.Infrastructure.EntityConfigurations;
using Ingweland.Fog.Models.Fog.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ingweland.Fog.Infrastructure;

public class FogDbContext : DbContext, IFogDbContext
{
    public FogDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<CommunityCityGuideEntity> CommunityCityGuides { get; set; }

    public DbSet<EventCityStrategy> EventCityStrategies { get; set; }

    public DbSet<EventCityFetchState> EventCityFetchStates { get; set; }

    public DbSet<EventCitySnapshot> EventCitySnapshots { get; set; }

    public DbSet<EventCityWonderRanking> EventCityWonderRankings { get; set; }

    public DbSet<HeroAbilityFeaturesEntity> HeroAbilityFeatures { get; set; }

    public DbSet<AnnualBudget> AnnualBudgets { get; set; }

    public DbSet<InGameEventEntity> InGameEvents { get; set; }

    public DbSet<PvpRanking2> PvpRankings { get; set; }
    public DbSet<RelicInsightsEntity> RelicInsights { get; set; }

    public DbSet<BattleTimelineEntity> BattleTimelines { get; set; }

    public DbSet<BattleUnitEntity> BattleUnits { get; set; }

    public DbSet<BattleSummaryEntity> Battles { get; set; }

    public DbSet<PlayerRanking> PlayerRankings { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Alliance> Alliances { get; set; }
    public DbSet<AllianceAthRanking> AllianceAthRankings { get; set; }
    public DbSet<AllianceWoaRanking> AllianceWoaRankings { get; set; }
    public DbSet<PlayerAthRanking> PlayerAthRankings { get; set; }
    public DbSet<AllianceRanking> AllianceRankings { get; set; }
    public DbSet<PvpBattle> PvpBattles { get; set; }
    public DbSet<TopHeroInsightsEntity> TopHeroInsights { get; set; }
    public DbSet<WoaPlayerStatsEntity> WoaPlayerStats { get; set; }
    public DbSet<BattleStatsEntity> BattleStats { get; set; }
    public DbSet<PlayerCitySnapshot> PlayerCitySnapshots { get; set; }
    public DbSet<ProfileSquadEntity> ProfileSquads { get; set; }
    public DbSet<SharedSubmissionIdEntity> SharedSubmissionIds { get; set; }
    public DbSet<ProfileSquadDataEntity> ProfileSquadDataItems { get; set; }
    public DbSet<EquipmentInsightsEntity> EquipmentInsights { get; set; }
    public DbSet<CommunityCityStrategyEntity> CommunityCityStrategies { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new PlayerEntityTypeConfiguration());
        builder.ApplyConfiguration(new PlayerRankingEntityTypeConfiguration());
        builder.ApplyConfiguration(new PvpRankingEntityTypeConfiguration());
        builder.ApplyConfiguration(new PlayerNameHistoryEntryEntityTypeConfiguration());
        builder.ApplyConfiguration(new PlayerAgeHistoryEntryEntityTypeConfiguration());
        builder.ApplyConfiguration(new AllianceEntityTypeConfiguration());
        builder.ApplyConfiguration(new AllianceRankingEntityTypeConfiguration());
        builder.ApplyConfiguration(new AllianceNameHistoryEntryEntityTypeConfiguration());
        builder.ApplyConfiguration(new PvpBattleEntityTypeConfiguration());
        builder.ApplyConfiguration(new BattleSummaryEntityTypeConfiguration());
        builder.ApplyConfiguration(new BattleUnitEntityTypeConfiguration());
        builder.ApplyConfiguration(new BattleStatsEntityTypeConfiguration());
        builder.ApplyConfiguration(new BattleSquadStatsEntityTypeConfiguration());
        builder.ApplyConfiguration(new UnitBattleStatsEntityTypeConfiguration());
        builder.ApplyConfiguration(new PlayerCitySnapshotEntityTypeConfiguration());
        builder.ApplyConfiguration(new ProfileSquadEntityTypeConfiguration());
        builder.ApplyConfiguration(new AllianceMemberEntityTypeConfiguration());
        builder.ApplyConfiguration(new TopHeroInsightsEntityTypeConfiguration());
        builder.ApplyConfiguration(new BattleTimelineEntityTypeConfiguration());
        builder.ApplyConfiguration(new BattleSquadsEntityTypeConfiguration());
        builder.ApplyConfiguration(new ProfileSquadDataEntityTypeConfiguration());
        builder.ApplyConfiguration(new SharedSubmissionIdEntityTypeConfiguration());
        builder.ApplyConfiguration(new EquipmentInsightsEntityTypeConfiguration());
        builder.ApplyConfiguration(new RelicInsightsEntityTypeConfiguration());
        builder.ApplyConfiguration(new PlayerCitySnapshotDataEntityTypeConfiguration());
        builder.ApplyConfiguration(new InGameEventEntityTypeConfiguration());
        builder.ApplyConfiguration(new AllianceAthRankingEntityTypeConfiguration());
        builder.ApplyConfiguration(new PvpRanking2EntityTypeConfiguration());
        builder.ApplyConfiguration(new PvpBattleTeamsEntityTypeConfiguration());
        builder.ApplyConfiguration(new AnnualBudgetEntityTypeConfiguration());
        builder.ApplyConfiguration(new HeroAbilityFeaturesEntityTypeConfiguration());
        builder.ApplyConfiguration(new EventCityWonderRankingEntityTypeConfiguration());
        builder.ApplyConfiguration(new EventCitySnapshotEntityTypeConfiguration());
        builder.ApplyConfiguration(new EventCitySnapshotDataEntityTypeConfiguration());
        builder.ApplyConfiguration(new EventCityFetchStateEntityTypeConfiguration());
        builder.ApplyConfiguration(new EventCityStrategyEntityTypeConfiguration());
        builder.ApplyConfiguration(new EventCityStrategyDataEntityTypeConfiguration());
        builder.ApplyConfiguration(new CommunityCityStrategyEntityTypeConfiguration());
        builder.ApplyConfiguration(new CommunityCityGuideEntityTypeConfiguration());
        builder.ApplyConfiguration(new PlayerAthRankingEntityTypeConfiguration());
        builder.ApplyConfiguration(new AllianceWoaRankingEntityTypeConfiguration());
        builder.ApplyConfiguration(new WoaPlayerStatsEntityTypeConfiguration());
    }
}
