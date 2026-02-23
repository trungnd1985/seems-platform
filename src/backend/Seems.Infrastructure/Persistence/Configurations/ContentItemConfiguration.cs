using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seems.Domain.Entities;

namespace Seems.Infrastructure.Persistence.Configurations;

public class ContentItemConfiguration : IEntityTypeConfiguration<ContentItem>
{
    public void Configure(EntityTypeBuilder<ContentItem> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.ContentTypeKey);
        builder.Property(e => e.ContentTypeKey).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Data).HasColumnType("jsonb");
        builder.HasIndex(e => e.Data).HasMethod("gin").HasDatabaseName("ix_content_items_data_gin");
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(32);
    }
}
