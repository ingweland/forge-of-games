using Ingweland.Fog.Application.Core.Constants;
using Ingweland.Fog.Models.Fog.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ingweland.Fog.Infrastructure.EntityConfigurations;

public class CommunityCityStrategyEntityTypeConfiguration : IEntityTypeConfiguration<CommunityCityStrategyEntity>
{
    public void Configure(EntityTypeBuilder<CommunityCityStrategyEntity> builder)
    {
        builder.ToTable("community_city_strategies");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(FogConstants.NAME_MAX_CHARACTERS);
        builder.Property(p => p.Author).IsRequired();
        builder.Property(p => p.SharedDataId).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();
        builder.Property(p => p.CityId).IsRequired().HasConversion<string>();
        builder.Property(p => p.WonderId).HasConversion<string>();
        builder.Property(p => p.IsEnabled).HasDefaultValue(true);
        builder.Property(p => p.AgeId);
    }
}
