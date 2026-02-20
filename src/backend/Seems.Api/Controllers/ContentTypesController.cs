using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Content.Dtos;
using Seems.Application.ContentTypes.Commands.CreateContentType;
using Seems.Application.ContentTypes.Commands.DeleteContentType;
using Seems.Application.ContentTypes.Commands.UpdateContentType;
using Seems.Application.ContentTypes.Queries.GetContentType;
using Seems.Application.ContentTypes.Queries.ListContentTypes;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/content-types")]
[Authorize]
public class ContentTypesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ContentTypeDto>>> List(CancellationToken ct)
        => Ok(await sender.Send(new ListContentTypesQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContentTypeDto>> Get(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetContentTypeQuery(id), ct));

    [HttpPost]
    public async Task<ActionResult<ContentTypeDto>> Create(
        [FromBody] CreateContentTypeRequest body,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateContentTypeCommand(body.Key, body.Name, body.Schema), ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ContentTypeDto>> Update(
        Guid id,
        [FromBody] UpdateContentTypeRequest body,
        CancellationToken ct)
        => Ok(await sender.Send(new UpdateContentTypeCommand(id, body.Name, body.Schema), ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteContentTypeCommand(id), ct);
        return NoContent();
    }
}

public record CreateContentTypeRequest(string Key, string Name, string Schema);
public record UpdateContentTypeRequest(string Name, string Schema);
