using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Content.Commands.CreateContentItem;
using Seems.Application.Content.Commands.DeleteContentItem;
using Seems.Application.Content.Commands.UpdateContentItem;
using Seems.Application.Content.Dtos;
using Seems.Application.Content.Queries.ListContentItems;

namespace Seems.Modules.Slider;

[ApiController]
[Route("api/modules/slider")]
public class SliderController(ISender sender) : ControllerBase
{
    private const string ContentTypeKey = "slider-slide";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>Returns published slides ordered by Order field. Used by the public Nuxt component.</summary>
    [HttpGet("slides")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<SlideDto>>> ListPublished(CancellationToken ct)
    {
        var result = await sender.Send(
            new ListContentItemsQuery(ContentTypeKey: ContentTypeKey, Status: "Published", PageSize: 100), ct);

        var slides = result.Items
            .Select(MapSlide)
            .OrderBy(s => s.Order)
            .ToList();

        return Ok(slides);
    }

    /// <summary>Returns all slides (any status). Admin only.</summary>
    [HttpGet("slides/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<SlideDto>>> ListAll(CancellationToken ct)
    {
        var result = await sender.Send(
            new ListContentItemsQuery(ContentTypeKey: ContentTypeKey, PageSize: 100), ct);

        var slides = result.Items
            .Select(MapSlide)
            .OrderBy(s => s.Order)
            .ToList();

        return Ok(slides);
    }

    /// <summary>Create a new slide.</summary>
    [HttpPost("slides")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SlideDto>> Create([FromBody] CreateSlideRequest body, CancellationToken ct)
    {
        var data = JsonSerializer.Serialize(new
        {
            title = body.Title,
            subtitle = body.Subtitle,
            imageUrl = body.ImageUrl,
            ctaText = body.CtaText,
            ctaLink = body.CtaLink,
            order = body.Order,
        }, JsonOpts);

        var item = await sender.Send(new CreateContentItemCommand(ContentTypeKey, data), ct);
        return CreatedAtAction(nameof(ListAll), MapSlide(item));
    }

    /// <summary>Update an existing slide, including status.</summary>
    [HttpPut("slides/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SlideDto>> Update(
        Guid id, [FromBody] UpdateSlideRequest body, CancellationToken ct)
    {
        var data = JsonSerializer.Serialize(new
        {
            title = body.Title,
            subtitle = body.Subtitle,
            imageUrl = body.ImageUrl,
            ctaText = body.CtaText,
            ctaLink = body.CtaLink,
            order = body.Order,
        }, JsonOpts);

        var item = await sender.Send(new UpdateContentItemCommand(id, data, body.Status), ct);
        return Ok(MapSlide(item));
    }

    /// <summary>Delete a slide.</summary>
    [HttpDelete("slides/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteContentItemCommand(id), ct);
        return NoContent();
    }

    // ─── Mapping helper ──────────────────────────────────────────────────────

    private static SlideDto MapSlide(ContentItemDto dto)
    {
        var slide = new SlideDto
        {
            Id = dto.Id,
            Status = dto.Status,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
        };

        try
        {
            var data = dto.Data;
            slide.Title = data.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
            slide.Subtitle = data.TryGetProperty("subtitle", out var s) ? s.GetString() : null;
            slide.ImageUrl = data.TryGetProperty("imageUrl", out var img) ? img.GetString() ?? "" : "";
            slide.CtaText = data.TryGetProperty("ctaText", out var ct) ? ct.GetString() : null;
            slide.CtaLink = data.TryGetProperty("ctaLink", out var cl) ? cl.GetString() : null;
            slide.Order = data.TryGetProperty("order", out var o) ? o.GetInt32() : 0;
        }
        catch
        {
            // Malformed data — return defaults already set above
        }

        return slide;
    }
}
