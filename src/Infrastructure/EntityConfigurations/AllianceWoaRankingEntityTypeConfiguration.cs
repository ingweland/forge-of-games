using Ingweland.Fog.Models.Fog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ingweland.Fog.Infrastructure.EntityConfigurations;

public class AllianceWoaRankingEntityTypeConfiguration : IEntityTypeConfiguration<AllianceWoaRanking>
{
    public void Configure(EntityTypeBuilder<AllianceWoaRanking> builder)
    {
        builder.ToTable("alliance_woa_rankings");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.EloRating).IsRequired();
        builder.Property(p => p.VictoryPoints).IsRequired();
        builder.Property(p => p.DivisionId).IsRequired();
        builder.Property(p => p.ExpectedVictoryPointsShare).IsRequired();
        builder.Property(p => p.CollectedAt).IsRequired();
        builder.Property(p => p.InGameEventId).IsRequired();

        builder.HasIndex(p => p.EloRating).IsDescending();
        builder.HasIndex(p => p.DivisionId);
        builder.HasIndex(p => p.InGameEventId);
        builder.HasIndex(p => new {p.AllianceId, p.InGameEventId, p.DivisionId}).IsUnique();
    }
}
