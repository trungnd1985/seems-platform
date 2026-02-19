using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Content.Commands.CreateContentItem;
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
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await sender.Send(new ListContentItemsQuery(page, pageSize));
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
    public async Task<ActionResult<ContentItemDto>> Create([FromBody] CreateContentItemCommand command)
    {
        var result = await sender.Send(command);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
}
