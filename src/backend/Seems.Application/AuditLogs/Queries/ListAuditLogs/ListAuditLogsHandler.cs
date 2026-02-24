using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Seems.Application.AuditLogs.Dtos;
using Seems.Application.Common.Interfaces;
using Seems.Application.Common.Models;
using Seems.Domain.Enums;

namespace Seems.Application.AuditLogs.Queries.ListAuditLogs;

public class ListAuditLogsHandler(IAppDbContext db)
    : IRequestHandler<ListAuditLogsQuery, PaginatedList<AuditLogDto>>
{
    public async Task<PaginatedList<AuditLogDto>> Handle(
        ListAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.EntityName))
            query = query.Where(x => x.EntityName == request.EntityName);

        if (!string.IsNullOrWhiteSpace(request.Action) &&
            Enum.TryParse<AuditAction>(request.Action, ignoreCase: true, out var action))
            query = query.Where(x => x.Action == action);

        if (!string.IsNullOrWhiteSpace(request.UserEmail))
            query = query.Where(x => x.UserEmail != null &&
                                     x.UserEmail.ToLower().Contains(request.UserEmail.ToLower()));

        if (request.DateFrom.HasValue)
            query = query.Where(x => x.Timestamp >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(x => x.Timestamp < request.DateTo.Value.AddDays(1));

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new AuditLogDto(
                x.Id,
                x.EntityName,
                x.EntityId,
                x.Action.ToString(),
                x.UserId,
                x.UserEmail,
                x.ChangedFields != null
                    ? JsonSerializer.Deserialize<string[]>(x.ChangedFields)
                    : null,
                x.Timestamp))
            .ToListAsync(cancellationToken);

        return new PaginatedList<AuditLogDto>(items, total, request.Page, request.PageSize);
    }
}
