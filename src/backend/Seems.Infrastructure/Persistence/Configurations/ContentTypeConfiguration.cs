using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seems.Domain.Entities;

namespace Seems.Infrastructure.Persistence.Configurations;

public class ContentTypeConfiguration : IEntityTypeConfiguration<ContentType>
{
    public void Configure(EntityTypeBuilder<ContentType> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Key).IsUnique();
        builder.Property(e => e.Key).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Schema).HasColumnType("json");
    }
}
