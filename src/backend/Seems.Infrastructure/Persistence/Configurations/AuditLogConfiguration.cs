using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Seems.Domain.Entities;

namespace Seems.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.EntityName).IsRequired().HasMaxLength(128);
        builder.Property(x => x.EntityId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Action).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.UserId).HasMaxLength(128);
        builder.Property(x => x.UserEmail).HasMaxLength(256);
        builder.Property(x => x.ChangedFields).HasColumnType("jsonb");

        builder.HasIndex(x => x.Timestamp).IsDescending();
        builder.HasIndex(x => x.EntityName);
    }
}
