using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seems.Domain.Entities;

namespace Seems.Infrastructure.Persistence.Configurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Slug).IsUnique();

        // Filtered unique index: at most one row may have IsDefault = true
        builder.HasIndex(e => e.IsDefault)
            .IsUnique()
            .HasFilter("\"IsDefault\" = true");

        builder.Property(e => e.Slug).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(512).IsRequired();
        builder.Property(e => e.TemplateKey).HasMaxLength(128).IsRequired();
        builder.Property(e => e.ThemeKey).HasMaxLength(128);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(32);

        builder.OwnsOne(e => e.Seo, seo =>
        {
            seo.ToJson("seo");
        });

        builder.HasMany(e => e.Slots)
            .WithOne(e => e.Page)
            .HasForeignKey(e => e.PageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
