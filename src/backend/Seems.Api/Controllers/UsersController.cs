using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Identity.Users.Commands.CreateUser;
using Seems.Application.Identity.Users.Commands.DeleteUser;
using Seems.Application.Identity.Users.Commands.LockUser;
using Seems.Application.Identity.Users.Commands.ResetUserPassword;
using Seems.Application.Identity.Users.Commands.UnlockUser;
using Seems.Application.Identity.Users.Commands.UpdateUser;
using Seems.Application.Identity.Users.Dtos;
using Seems.Application.Identity.Users.Queries.GetUser;
using Seems.Application.Identity.Users.Queries.ListUsers;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await sender.Send(new ListUsersQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDetailDto>> Get(Guid id)
    {
        var result = await sender.Send(new GetUserQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserDetailDto>> Create([FromBody] CreateUserCommand command)
    {
        var result = await sender.Send(command);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDetailDto>> Update(Guid id, [FromBody] UpdateUserBody body)
    {
        var result = await sender.Send(new UpdateUserCommand(id, body.Email, body.DisplayName, body.Roles));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await sender.Send(new DeleteUserCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordBody body)
    {
        await sender.Send(new ResetUserPasswordCommand(id, body.NewPassword));
        return NoContent();
    }

    [HttpPost("{id:guid}/lock")]
    public async Task<IActionResult> Lock(Guid id)
    {
        await sender.Send(new LockUserCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/unlock")]
    public async Task<IActionResult> Unlock(Guid id)
    {
        await sender.Send(new UnlockUserCommand(id));
        return NoContent();
    }
}

public record UpdateUserBody(string Email, string DisplayName, List<string> Roles);
public record ResetPasswordBody(string NewPassword);
