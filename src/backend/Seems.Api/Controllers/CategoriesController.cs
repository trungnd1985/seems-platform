using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seems.Application.Categories.Commands.CreateCategory;
using Seems.Application.Categories.Commands.DeleteCategory;
using Seems.Application.Categories.Commands.UpdateCategory;
using Seems.Application.Categories.Dtos;
using Seems.Application.Categories.Queries.GetCategoryTree;
using Seems.Application.Categories.Queries.ListCategories;

namespace Seems.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> List([FromQuery] string contentTypeKey)
    {
        if (string.IsNullOrWhiteSpace(contentTypeKey))
            return BadRequest("contentTypeKey is required.");

        var result = await sender.Send(new ListCategoriesQuery(contentTypeKey));
        return Ok(result);
    }

    [HttpGet("tree")]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> Tree([FromQuery] string contentTypeKey)
    {
        if (string.IsNullOrWhiteSpace(contentTypeKey))
            return BadRequest("contentTypeKey is required.");

        var result = await sender.Send(new GetCategoryTreeQuery(contentTypeKey));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryRequest request)
    {
        var result = await sender.Send(new CreateCategoryCommand(
            request.Name,
            request.Slug,
            request.Description,
            request.ContentTypeKey,
            request.ParentId,
            request.SortOrder));

        return CreatedAtAction(nameof(List), new { contentTypeKey = result.ContentTypeKey }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await sender.Send(new UpdateCategoryCommand(
            id,
            request.Name,
            request.Slug,
            request.Description,
            request.ParentId,
            request.SortOrder));

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await sender.Send(new DeleteCategoryCommand(id));
        return NoContent();
    }
}

public record CreateCategoryRequest(
    string Name,
    string? Slug,
    string? Description,
    string ContentTypeKey,
    Guid? ParentId,
    int SortOrder = 0);

public record UpdateCategoryRequest(
    string Name,
    string? Slug,
    string? Description,
    Guid? ParentId,
    int SortOrder = 0);
