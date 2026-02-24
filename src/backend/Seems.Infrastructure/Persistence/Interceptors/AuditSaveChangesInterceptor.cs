using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Seems.Application.Common.Interfaces;
using Seems.Domain.Common;
using Seems.Domain.Entities;
using Seems.Domain.Enums;

namespace Seems.Infrastructure.Persistence.Interceptors;

public class AuditSaveChangesInterceptor(ICurrentUser currentUser) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            AddAuditLogs(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditLogs(DbContext context)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditable &&
                        e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (entries.Count == 0) return;

        var now = DateTime.UtcNow;
        var userId = currentUser.UserId?.ToString();
        var userEmail = currentUser.Email;

        foreach (var entry in entries)
        {
            context.Set<AuditLog>().Add(new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                EntityId = GetEntityId(entry),
                Action = entry.State switch
                {
                    EntityState.Added => AuditAction.Created,
                    EntityState.Modified => AuditAction.Updated,
                    EntityState.Deleted => AuditAction.Deleted,
                    _ => AuditAction.Updated,
                },
                UserId = userId,
                UserEmail = userEmail,
                ChangedFields = entry.State == EntityState.Modified
                    ? JsonSerializer.Serialize(
                        entry.Properties
                            .Where(p => p.IsModified)
                            .Select(p => p.Metadata.Name)
                            .ToArray())
                    : null,
                Timestamp = now,
            });
        }
    }

    private static string GetEntityId(EntityEntry entry)
    {
        var keyValues = entry.Metadata.FindPrimaryKey()
            ?.Properties
            .Select(p => entry.Property(p.Name).CurrentValue?.ToString())
            .ToList();

        return keyValues is { Count: > 0 } ? string.Join(",", keyValues) : string.Empty;
    }
}
