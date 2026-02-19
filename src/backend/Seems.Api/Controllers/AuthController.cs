using MediatR;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Identity.Commands.Login;
using Seems.Application.Identity.Dtos;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await sender.Send(new LoginCommand(request.Email, request.Password));
        return Ok(result);
    }
}
