using Seems.Domain.Enums;

namespace Seems.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? ChangedFields { get; set; }
    public DateTime Timestamp { get; set; }
}
