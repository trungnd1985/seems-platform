using MediatR;
using Seems.Application.Categories.Dtos;

namespace Seems.Application.Categories.Queries.GetCategoryTree;

public record GetCategoryTreeQuery(string ContentTypeKey) : IRequest<IReadOnlyList<CategoryDto>>;
