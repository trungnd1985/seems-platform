using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seems.Domain.Entities;

namespace Seems.Infrastructure.Persistence.Configurations;

public class SlotMappingConfiguration : IEntityTypeConfiguration<SlotMapping>
{
    public void Configure(EntityTypeBuilder<SlotMapping> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SlotKey).HasMaxLength(128).IsRequired();
        builder.Property(e => e.TargetType).HasConversion<string>().HasMaxLength(32);
        builder.Property(e => e.TargetId).HasMaxLength(256).IsRequired();
        builder.HasIndex(e => new { e.PageId, e.SlotKey, e.Order });
    }
}
