using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seems.Domain.Entities;

namespace Seems.Infrastructure.Persistence.Configurations;

public class MediaFolderConfiguration : IEntityTypeConfiguration<MediaFolder>
{
    public void Configure(EntityTypeBuilder<MediaFolder> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).HasMaxLength(128).IsRequired();

        // No duplicate names within the same parent and owner
        builder.HasIndex(e => new { e.OwnerId, e.ParentId, e.Name }).IsUnique();

        // Self-referencing: children are cascade-deleted when parent is deleted
        builder.HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.OwnerId);
    }
}
