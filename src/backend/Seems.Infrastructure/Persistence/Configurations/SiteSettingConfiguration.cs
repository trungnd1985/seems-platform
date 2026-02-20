using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seems.Domain.Entities;

namespace Seems.Infrastructure.Persistence.Configurations;

public class SiteSettingConfiguration : IEntityTypeConfiguration<SiteSetting>
{
    public void Configure(EntityTypeBuilder<SiteSetting> builder)
    {
        builder.HasKey(e => e.Key);
        builder.Property(e => e.Key).HasMaxLength(128);
        builder.Property(e => e.Group).HasMaxLength(64).IsRequired();
        builder.Property(e => e.Value).IsRequired();
    }
}
