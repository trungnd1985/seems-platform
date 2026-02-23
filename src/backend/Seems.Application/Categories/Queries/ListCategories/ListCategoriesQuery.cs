using MediatR;
using Seems.Application.Categories.Dtos;

namespace Seems.Application.Categories.Queries.ListCategories;

public record ListCategoriesQuery(string ContentTypeKey) : IRequest<IReadOnlyList<CategoryDto>>;
