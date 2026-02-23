using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Content.Commands.CreateContentItem;
using Seems.Application.Content.Commands.DeleteContentItem;
using Seems.Application.Content.Commands.UpdateContentItem;
using Seems.Application.Content.Dtos;
using Seems.Application.Content.Queries.GetContentItem;
using Seems.Application.Content.Queries.ListContentItems;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/content-items")]
public class ContentItemsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? contentTypeKey = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null)
    {
        var result = await sender.Send(new ListContentItemsQuery(page, pageSize, contentTypeKey, status, categoryId, search));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContentItemDto>> Get(Guid id)
    {
        var result = await sender.Send(new GetContentItemQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ContentItemDto>> Create([FromBody] CreateContentItemRequest request)
    {
        var result = await sender.Send(new CreateContentItemCommand(request.ContentTypeKey, request.Data, request.CategoryIds));
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ContentItemDto>> Update(Guid id, [FromBody] UpdateContentItemRequest request)
    {
        var result = await sender.Send(new UpdateContentItemCommand(id, request.Data, request.Status, request.CategoryIds));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        await sender.Send(new DeleteContentItemCommand(id));
        return NoContent();
    }
}

public record CreateContentItemRequest(string ContentTypeKey, string Data, IEnumerable<Guid>? CategoryIds = null);
public record UpdateContentItemRequest(string Data, string? Status, IEnumerable<Guid>? CategoryIds = null);
