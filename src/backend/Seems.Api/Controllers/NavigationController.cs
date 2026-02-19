using Microsoft.AspNetCore.Mvc;
using Seems.Domain.Enums;
using Seems.Domain.Interfaces;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NavigationController(IPageRepository pageRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var pages = await pageRepository.GetPublishedPagesAsync();
        var nav = pages
            .Where(p => p.Slug != "/")
            .Select(p => new { label = p.Title, slug = p.Slug })
            .ToList();
        return Ok(nav);
    }
}
