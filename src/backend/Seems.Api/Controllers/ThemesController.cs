using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Themes.Commands.CreateTheme;
using Seems.Application.Themes.Commands.DeleteTheme;
using Seems.Application.Themes.Commands.UpdateTheme;
using Seems.Application.Themes.Dtos;
using Seems.Application.Themes.Queries.GetTheme;
using Seems.Application.Themes.Queries.ListThemes;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/themes")]
[Authorize]
public class ThemesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ThemeDto>>> List(CancellationToken ct)
        => Ok(await sender.Send(new ListThemesQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ThemeDto>> Get(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetThemeQuery(id), ct));

    [HttpPost]
    public async Task<ActionResult<ThemeDto>> Create(
        [FromBody] CreateThemeRequest body,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateThemeCommand(body.Key, body.Name, body.Description), ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ThemeDto>> Update(
        Guid id,
        [FromBody] UpdateThemeRequest body,
        CancellationToken ct)
        => Ok(await sender.Send(new UpdateThemeCommand(id, body.Name, body.Description), ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteThemeCommand(id), ct);
        return NoContent();
    }
}

public record CreateThemeRequest(string Key, string Name, string? Description);
public record UpdateThemeRequest(string Name, string? Description);
