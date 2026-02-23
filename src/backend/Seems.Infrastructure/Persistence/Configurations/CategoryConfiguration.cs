using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seems.Domain.Entities;

namespace Seems.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Slug).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1024);
        builder.Property(e => e.ContentTypeKey).HasMaxLength(128).IsRequired();
        builder.Property(e => e.SortOrder).HasDefaultValue(0);

        // Composite unique: slug is unique within the same (ContentTypeKey, ParentId) scope
        builder.HasIndex(e => new { e.ContentTypeKey, e.ParentId, e.Slug }).IsUnique();
        builder.HasIndex(e => e.ContentTypeKey);

        builder.HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
