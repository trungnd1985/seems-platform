using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.AuditLogs.Dtos;
using Seems.Application.AuditLogs.Queries.ListAuditLogs;
using Seems.Application.Common.Models;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "Admin")]
public class AuditLogsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedList<AuditLogDto>>> List([FromQuery] ListAuditLogsRequest request)
    {
        var result = await sender.Send(new ListAuditLogsQuery(
            request.EntityName,
            request.Action,
            request.UserEmail,
            request.DateFrom,
            request.DateTo,
            request.Page,
            request.PageSize));

        return Ok(result);
    }
}

public record ListAuditLogsRequest(
    string? EntityName = null,
    string? Action = null,
    string? UserEmail = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    int Page = 1,
    int PageSize = 50);
