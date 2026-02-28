using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Modules.Commands.DeleteModule;
using Seems.Application.Modules.Commands.RegisterModule;
using Seems.Application.Modules.Commands.SetModuleStatus;
using Seems.Application.Modules.Commands.UpdateModule;
using Seems.Application.Modules.Dtos;
using Seems.Application.Modules.Queries.ListInstalledModules;
using Seems.Application.Modules.Queries.ListModules;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModulesController(ISender sender) : ControllerBase
{
    /// <summary>Admin list — all modules regardless of status.</summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<ModuleDto>>> List(CancellationToken ct)
        => Ok(await sender.Send(new ListModulesQuery(), ct));

    /// <summary>
    /// Public endpoint consumed by the Nuxt plugin at app startup.
    /// Returns only Installed modules that have a PublicComponentUrl.
    /// </summary>
    [HttpGet("installed")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<InstalledModuleDto>>> ListInstalled(CancellationToken ct)
        => Ok(await sender.Send(new ListInstalledModulesQuery(), ct));

    /// <summary>Register and install a new module.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ModuleDto>> Register([FromBody] RegisterModuleCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return CreatedAtAction(nameof(List), new { }, result);
    }

    /// <summary>Update module metadata (name, version, publicComponentUrl, description, author).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ModuleDto>> Update(
        Guid id, [FromBody] UpdateModuleBody body, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateModuleCommand(id, body.Name, body.Version, body.PublicComponentUrl, body.Description, body.Author), ct);
        return Ok(result);
    }

    /// <summary>Enable or disable an installed module.</summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ModuleDto>> SetStatus(
        Guid id, [FromBody] SetModuleStatusBody body, CancellationToken ct)
    {
        var result = await sender.Send(new SetModuleStatusCommand(id, body.Status), ct);
        return Ok(result);
    }

    /// <summary>Uninstall a module. Orphaned slot mappings are removed automatically.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteModuleCommand(id), ct);
        return NoContent();
    }
}

public record SetModuleStatusBody(string Status);
public record UpdateModuleBody(
    string Name,
    string Version,
    string? PublicComponentUrl,
    string? Description,
    string? Author);
