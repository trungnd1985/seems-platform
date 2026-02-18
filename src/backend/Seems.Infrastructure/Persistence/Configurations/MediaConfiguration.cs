using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Seems.Infrastructure.Persistence.Configurations;

public class MediaConfiguration : IEntityTypeConfiguration<Domain.Entities.Media>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Media> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FileName).HasMaxLength(512).IsRequired();
        builder.Property(e => e.Url).HasMaxLength(1024).IsRequired();
        builder.Property(e => e.MimeType).HasMaxLength(128).IsRequired();
    }
}
