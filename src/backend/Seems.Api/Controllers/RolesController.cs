using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Identity.Roles.Commands.CreateRole;
using Seems.Application.Identity.Roles.Commands.DeleteRole;
using Seems.Application.Identity.Roles.Commands.UpdateRole;
using Seems.Application.Identity.Roles.Dtos;
using Seems.Application.Identity.Roles.Queries.GetRole;
using Seems.Application.Identity.Roles.Queries.ListRoles;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = "Admin")]
public class RolesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RoleDto>>> List()
    {
        var result = await sender.Send(new ListRolesQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoleDto>> Get(Guid id)
    {
        var result = await sender.Send(new GetRoleQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleCommand command)
    {
        var result = await sender.Send(command);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RoleDto>> Update(Guid id, [FromBody] UpdateRoleBody body)
    {
        var result = await sender.Send(new UpdateRoleCommand(id, body.Name, body.Description));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await sender.Send(new DeleteRoleCommand(id));
        return NoContent();
    }
}

public record UpdateRoleBody(string Name, string? Description);
