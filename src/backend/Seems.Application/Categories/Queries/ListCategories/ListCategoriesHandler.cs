using AutoMapper;
using MediatR;
using Seems.Application.Categories.Dtos;
using Seems.Domain.Interfaces;

namespace Seems.Application.Categories.Queries.ListCategories;

public class ListCategoriesHandler(ICategoryRepository categoryRepository, IMapper mapper)
    : IRequestHandler<ListCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public async Task<IReadOnlyList<CategoryDto>> Handle(ListCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetByContentTypeKeyAsync(request.ContentTypeKey, cancellationToken);
        return categories.Select(c => mapper.Map<CategoryDto>(c)).ToList();
    }
}
