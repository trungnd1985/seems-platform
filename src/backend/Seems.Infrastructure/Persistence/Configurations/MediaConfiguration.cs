using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Seems.Infrastructure.Persistence.Configurations;

public class MediaConfiguration : IEntityTypeConfiguration<Domain.Entities.Media>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Media> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FileName).HasMaxLength(512).IsRequired();
        builder.Property(e => e.OriginalName).HasMaxLength(512).IsRequired();
        builder.Property(e => e.StorageKey).HasMaxLength(1024).IsRequired();
        builder.Property(e => e.Url).HasMaxLength(1024).IsRequired();
        builder.Property(e => e.MimeType).HasMaxLength(128).IsRequired();
        builder.Property(e => e.AltText).HasMaxLength(512);
        builder.Property(e => e.Caption).HasMaxLength(1024);

        builder.HasOne(e => e.Folder)
            .WithMany(f => f.Items)
            .HasForeignKey(e => e.FolderId)
            .OnDelete(DeleteBehavior.SetNull);

        // OwnerId references AspNetUsers but no FK navigation needed
        builder.HasIndex(e => e.OwnerId);
        builder.HasIndex(e => e.FolderId);
    }
}
