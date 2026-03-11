using MediatR;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Pages.Queries.GetNavigationPages;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NavigationController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var nav = await sender.Send(new GetNavigationPagesQuery(), cancellationToken);
        return Ok(nav);
    }
}
