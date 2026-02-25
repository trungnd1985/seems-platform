using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Pages.Commands.AddPageSlot;
using Seems.Application.Pages.Commands.CreatePage;
using Seems.Application.Pages.Commands.DeletePage;
using Seems.Application.Pages.Commands.RemovePageSlot;
using Seems.Application.Pages.Commands.ReorderPageSlots;
using Seems.Application.Pages.Commands.SetDefaultPage;
using Seems.Application.Pages.Commands.UpdatePage;
using Seems.Application.Pages.Commands.UpdatePageStatus;
using Seems.Application.Pages.Dtos;
using Seems.Application.Pages.Queries.GetDefaultPage;
using Seems.Application.Pages.Queries.GetPageById;
using Seems.Application.Pages.Queries.GetPageBySlug;
using Seems.Application.Pages.Queries.GetPageTree;
using Seems.Application.Pages.Queries.ListPages;
using Seems.Domain.Enums;

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

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<PageDto>> GetById(Guid id)
    {
        var result = await sender.Send(new GetPageByIdQuery(id));
        return Ok(result);
    }

    [HttpGet("default")]
    public async Task<ActionResult<PageDto>> GetDefault()
    {
        var result = await sender.Send(new GetDefaultPageQuery());
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("by-slug/{*slug}")]
    public async Task<ActionResult<PageDto>> GetBySlug(string slug)
    {
        var result = await sender.Send(new GetPageBySlugQuery(slug));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("tree")]
    [Authorize]
    public async Task<IActionResult> GetTree()
    {
        var result = await sender.Send(new GetPageTreeQuery());
        return Ok(result);
    }

    [HttpGet("sitemap")]
    public async Task<IActionResult> Sitemap()
    {
        var result = await sender.Send(new GetPageTreeQuery());
        var sitemap = result.Select(p => new { p.Path, p.UpdatedAt });
        return Ok(sitemap);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PageDto>> Create([FromBody] CreatePageCommand command)
    {
        var result = await sender.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<PageDto>> Update(Guid id, [FromBody] UpdatePageCommand command)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match body id.");

        var result = await sender.Send(command);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize]
    public async Task<ActionResult<PageDto>> UpdateStatus(Guid id, [FromBody] UpdatePageStatusRequest request)
    {
        var result = await sender.Send(new UpdatePageStatusCommand(id, request.Status));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await sender.Send(new DeletePageCommand(id));
        return NoContent();
    }

    [HttpPatch("{id:guid}/set-default")]
    [Authorize]
    public async Task<ActionResult<PageDto>> SetDefault(Guid id)
    {
        var result = await sender.Send(new SetDefaultPageCommand(id));
        return Ok(result);
    }

    // --- Slot composition ---

    [HttpPost("{pageId:guid}/slots")]
    [Authorize]
    public async Task<ActionResult<SlotMappingDto>> AddSlot(Guid pageId, [FromBody] AddPageSlotCommand command)
    {
        if (pageId != command.PageId)
            return BadRequest("Route pageId does not match body pageId.");

        var result = await sender.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = pageId }, result);
    }

    [HttpDelete("{pageId:guid}/slots/{slotId:guid}")]
    [Authorize]
    public async Task<IActionResult> RemoveSlot(Guid pageId, Guid slotId)
    {
        await sender.Send(new RemovePageSlotCommand(pageId, slotId));
        return NoContent();
    }

    [HttpPatch("{pageId:guid}/slots/order")]
    [Authorize]
    public async Task<IActionResult> ReorderSlots(Guid pageId, [FromBody] IReadOnlyList<SlotOrderItem> items)
    {
        await sender.Send(new ReorderPageSlotsCommand(pageId, items));
        return NoContent();
    }
}

public record UpdatePageStatusRequest(ContentStatus Status);
