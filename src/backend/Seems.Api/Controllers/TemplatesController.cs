using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Templates.Commands.CreateTemplate;
using Seems.Application.Templates.Commands.DeleteTemplate;
using Seems.Application.Templates.Commands.UpdateTemplate;
using Seems.Application.Templates.Dtos;
using Seems.Application.Templates.Queries.GetTemplate;
using Seems.Application.Templates.Queries.ListTemplates;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/templates")]
[Authorize]
public class TemplatesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TemplateDto>>> List(CancellationToken ct)
        => Ok(await sender.Send(new ListTemplatesQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TemplateDto>> Get(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetTemplateQuery(id), ct));

    [HttpPost]
    public async Task<ActionResult<TemplateDto>> Create(
        [FromBody] CreateTemplateRequest body,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CreateTemplateCommand(body.Key, body.Name, body.ThemeKey, body.Slots), ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TemplateDto>> Update(
        Guid id,
        [FromBody] UpdateTemplateRequest body,
        CancellationToken ct)
        => Ok(await sender.Send(
            new UpdateTemplateCommand(id, body.Name, body.ThemeKey, body.Slots), ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteTemplateCommand(id), ct);
        return NoContent();
    }
}

public record CreateTemplateRequest(
    string Key,
    string Name,
    string ThemeKey,
    IReadOnlyList<TemplateSlotDef> Slots);

public record UpdateTemplateRequest(
    string Name,
    string ThemeKey,
    IReadOnlyList<TemplateSlotDef> Slots);
