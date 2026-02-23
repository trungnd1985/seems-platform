using MediatR;
using Seems.Application.Categories.Dtos;

namespace Seems.Application.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Slug,
    string? Description,
    Guid? ParentId,
    int SortOrder = 0
) : IRequest<CategoryDto>;
