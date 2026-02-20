using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Settings.Commands.UpdateStorageSettings;
using Seems.Application.Settings.Dtos;
using Seems.Application.Settings.Queries.GetStorageSettings;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize(Roles = "Admin")]
public class SettingsController(ISender sender) : ControllerBase
{
    [HttpGet("storage")]
    public async Task<ActionResult<StorageSettingsDto>> GetStorage()
    {
        var result = await sender.Send(new GetStorageSettingsQuery());
        return Ok(result);
    }

    [HttpPut("storage")]
    public async Task<IActionResult> UpdateStorage([FromBody] StorageSettingsDto dto)
    {
        await sender.Send(new UpdateStorageSettingsCommand(dto));
        return NoContent();
    }
}
