using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Pages.Commands.CreatePage;
using Seems.Application.Pages.Dtos;
using Seems.Application.Pages.Queries.GetPageBySlug;
using Seems.Application.Pages.Queries.ListPages;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await sender.Send(new ListPagesQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("by-slug/{*slug}")]
    public async Task<ActionResult<PageDto>> GetBySlug(string slug)
    {
        var result = await sender.Send(new GetPageBySlugQuery(slug));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("sitemap")]
    public async Task<IActionResult> Sitemap()
    {
        var result = await sender.Send(new ListPagesQuery(1, 10000));
        var sitemap = result.Items.Select(p => new { p.Slug, p.UpdatedAt });
        return Ok(sitemap);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PageDto>> Create([FromBody] CreatePageCommand command)
    {
        var result = await sender.Send(command);
        return CreatedAtAction(nameof(GetBySlug), new { slug = result.Slug }, result);
    }
}
