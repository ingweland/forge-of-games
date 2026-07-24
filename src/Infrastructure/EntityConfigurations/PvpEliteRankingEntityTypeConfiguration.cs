using Ingweland.Fog.Models.Fog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ingweland.Fog.Infrastructure.EntityConfigurations;

public class PvpEliteRankingEntityTypeConfiguration : IEntityTypeConfiguration<PvpEliteRanking>
{
    public void Configure(EntityTypeBuilder<PvpEliteRanking> builder)
    {
        builder.ToTable("pvp_elite_rankings");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Tier).IsRequired();
        builder.Property(p => p.CollectedAt).IsRequired();

        builder.HasIndex(p => p.CollectedAt).IsDescending();
    }
}
