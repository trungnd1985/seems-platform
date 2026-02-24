using MediatR;
using Seems.Application.AuditLogs.Dtos;
using Seems.Application.Common.Models;

namespace Seems.Application.AuditLogs.Queries.ListAuditLogs;

public record ListAuditLogsQuery(
    string? EntityName,
    string? Action,
    string? UserEmail,
    DateTime? DateFrom,
    DateTime? DateTo,
    int Page = 1,
    int PageSize = 50) : IRequest<PaginatedList<AuditLogDto>>;
