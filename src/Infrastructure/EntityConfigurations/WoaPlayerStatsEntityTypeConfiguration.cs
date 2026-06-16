using Ingweland.Fog.Models.Fog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ingweland.Fog.Infrastructure.EntityConfigurations;

public class WoaPlayerStatsEntityTypeConfiguration : IEntityTypeConfiguration<WoaPlayerStatsEntity>
{
    public void Configure(EntityTypeBuilder<WoaPlayerStatsEntity> builder)
    {
        builder.ToTable("woa_player_stats");

        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.CollectedAt).IsRequired();
        builder.Property(p => p.InGameEventId).IsRequired();

        builder.HasIndex(p => p.ContributionPoints).IsDescending(true);
        builder.HasIndex(x => new { x.InGameEventId, x.ContributionPoints }).IsDescending(false, true);
        builder.HasIndex(p => new {p.PlayerId, p.InGameEventId}).IsUnique();
    }
}
