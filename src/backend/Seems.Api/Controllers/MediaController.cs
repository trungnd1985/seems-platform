using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Common.Models;
using Seems.Application.Media.Commands.CreateMediaFolder;
using Seems.Application.Media.Commands.DeleteMedia;
using Seems.Application.Media.Commands.DeleteMediaFolder;
using Seems.Application.Media.Commands.MoveMedia;
using Seems.Application.Media.Commands.RenameMediaFolder;
using Seems.Application.Media.Commands.UploadMedia;
using Seems.Application.Media.Dtos;
using Seems.Application.Media.Queries.GetMedia;
using Seems.Application.Media.Queries.ListMedia;
using Seems.Application.Media.Queries.ListMediaFolders;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/media")]
[Authorize(Roles = "Admin,Editor")]
public class MediaController(ISender sender) : ControllerBase
{
    // ── Files ─────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<ActionResult<PaginatedList<MediaDto>>> List(
        [FromQuery] Guid? folderId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 40)
    {
        var result = await sender.Send(new ListMediaQuery(folderId, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MediaDto>> Get(Guid id)
    {
        var result = await sender.Send(new GetMediaQuery(id));
        return Ok(result);
    }

    [HttpPost("upload")]
    [RequestSizeLimit(52_428_800)] // 50 MB
    public async Task<ActionResult<MediaDto>> Upload(
        IFormFile file,
        [FromQuery] Guid? folderId)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var result = await sender.Send(new UploadMediaCommand(
            ms.ToArray(),
            file.FileName,
            file.ContentType,
            file.Length,
            folderId));

        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await sender.Send(new DeleteMediaCommand(id));
        return NoContent();
    }

    [HttpPatch("{id:guid}/move")]
    public async Task<ActionResult<MediaDto>> Move(Guid id, [FromBody] MoveMediaBody body)
    {
        var result = await sender.Send(new MoveMediaCommand(id, body.TargetFolderId));
        return Ok(result);
    }

    // ── Folders ───────────────────────────────────────────────────────────

    [HttpGet("folders")]
    public async Task<ActionResult<List<MediaFolderDto>>> ListFolders()
    {
        var result = await sender.Send(new ListMediaFoldersQuery());
        return Ok(result);
    }

    [HttpPost("folders")]
    public async Task<ActionResult<MediaFolderDto>> CreateFolder([FromBody] CreateMediaFolderCommand command)
    {
        var result = await sender.Send(command);
        return Created(string.Empty, result);
    }

    [HttpPut("folders/{id:guid}")]
    public async Task<ActionResult<MediaFolderDto>> RenameFolder(Guid id, [FromBody] RenameFolderBody body)
    {
        var result = await sender.Send(new RenameMediaFolderCommand(id, body.Name));
        return Ok(result);
    }

    [HttpDelete("folders/{id:guid}")]
    public async Task<IActionResult> DeleteFolder(Guid id)
    {
        await sender.Send(new DeleteMediaFolderCommand(id));
        return NoContent();
    }
}

public record MoveMediaBody(Guid? TargetFolderId);
public record RenameFolderBody(string Name);
