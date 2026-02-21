using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}
