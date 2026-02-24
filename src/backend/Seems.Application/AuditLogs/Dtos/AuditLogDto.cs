namespace Seems.Application.AuditLogs.Dtos;

public record AuditLogDto(
    long Id,
    string EntityName,
    string EntityId,
    string Action,
    string? UserId,
    string? UserEmail,
    string[]? ChangedFields,
    DateTime Timestamp);
