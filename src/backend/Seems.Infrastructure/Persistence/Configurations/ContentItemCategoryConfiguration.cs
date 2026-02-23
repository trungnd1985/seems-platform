using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seems.Domain.Entities;

namespace Seems.Infrastructure.Persistence.Configurations;

public class ContentItemCategoryConfiguration : IEntityTypeConfiguration<ContentItemCategory>
{
    public void Configure(EntityTypeBuilder<ContentItemCategory> builder)
    {
        builder.HasKey(e => new { e.ContentItemId, e.CategoryId });

        builder.HasOne(e => e.ContentItem)
            .WithMany(e => e.ContentItemCategories)
            .HasForeignKey(e => e.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Category)
            .WithMany(e => e.ContentItemCategories)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
