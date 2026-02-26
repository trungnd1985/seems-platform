using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Settings.Commands.UpdateSiteInfo;
using Seems.Application.Settings.Commands.UpdateStorageSettings;
using Seems.Application.Settings.Dtos;
using Seems.Application.Settings.Queries.GetSiteInfo;
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

    [HttpGet("site")]
    public async Task<ActionResult<SiteInfoDto>> GetSiteInfo()
    {
        var result = await sender.Send(new GetSiteInfoQuery());
        return Ok(result);
    }

    [HttpPut("site")]
    public async Task<IActionResult> UpdateSiteInfo([FromBody] SiteInfoDto dto)
    {
        await sender.Send(new UpdateSiteInfoCommand(dto));
        return NoContent();
    }
}
