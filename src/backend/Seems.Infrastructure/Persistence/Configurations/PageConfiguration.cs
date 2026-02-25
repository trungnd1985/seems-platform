using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seems.Domain.Entities;

namespace Seems.Infrastructure.Persistence.Configurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.HasKey(e => e.Id);

        // Path is the computed full URL (e.g. company/careers) — globally unique
        builder.HasIndex(e => e.Path).IsUnique();

        // Perf index for sibling lookups; path uniqueness enforces true uniqueness
        builder.HasIndex(e => new { e.ParentId, e.Slug });

        // Filtered unique index: at most one row may have IsDefault = true
        builder.HasIndex(e => e.IsDefault)
            .IsUnique()
            .HasFilter("\"IsDefault\" = true");

        builder.Property(e => e.Slug).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Path).HasMaxLength(2048).IsRequired();
        builder.Property(e => e.SortOrder).HasDefaultValue(0);
        builder.Property(e => e.Title).HasMaxLength(512).IsRequired();
        builder.Property(e => e.TemplateKey).HasMaxLength(128).IsRequired();
        builder.Property(e => e.ThemeKey).HasMaxLength(128);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(e => e.ShowInNavigation).HasDefaultValue(true);

        builder.OwnsOne(e => e.Seo, seo =>
        {
            seo.ToJson("seo");
        });

        // Self-referencing hierarchy — restrict at DB level so app layer can return 409
        builder.HasMany(e => e.Children)
            .WithOne(e => e.Parent)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Slots)
            .WithOne(e => e.Page)
            .HasForeignKey(e => e.PageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
