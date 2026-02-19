using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Templates.Dtos;
using Seems.Application.Templates.Queries.ListTemplates;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TemplatesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TemplateDto>>> List()
    {
        var result = await sender.Send(new ListTemplatesQuery());
        return Ok(result);
    }
}
