using Ingweland.Fog.Models.Fog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ingweland.Fog.Infrastructure.EntityConfigurations;

public class PlayerHeroEntityTypeConfiguration : IEntityTypeConfiguration<PlayerHeroEntity>
{
    public void Configure(EntityTypeBuilder<PlayerHeroEntity> builder)
    {
        builder.ToTable("player_heroes");

        builder.HasKey(p => p.Id);

        builder.Ignore(p => p.Key);

        builder.Property(p => p.UnitId).IsRequired();

        builder.HasIndex(p => p.UnitId);
        builder.HasIndex(p => new {p.PlayerId, p.UnitId}).IsUnique();
    }
}
