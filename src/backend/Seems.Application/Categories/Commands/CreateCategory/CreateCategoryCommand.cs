using MediatR;
using Seems.Application.Categories.Dtos;

namespace Seems.Application.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string? Slug,
    string? Description,
    string ContentTypeKey,
    Guid? ParentId,
    int SortOrder = 0
) : IRequest<CategoryDto>;
