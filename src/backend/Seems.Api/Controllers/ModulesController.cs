using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ModulesController(IRepository<Module> repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var items = await repository.GetAllAsync();
        return Ok(items);
    }
}
