using AutoMapper;
using MediatR;
using Seems.Application.Categories.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Categories.Queries.GetCategoryTree;

public class GetCategoryTreeHandler(ICategoryRepository categoryRepository, IMapper mapper)
    : IRequestHandler<GetCategoryTreeQuery, IReadOnlyList<CategoryDto>>
{
    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
    {
        var all = await categoryRepository.GetByContentTypeKeyAsync(request.ContentTypeKey, cancellationToken);
        var itemCountById = all.ToDictionary(
            c => c.Id,
            c => c.ContentItemCategories.Count);

        return BuildTree(all, null, itemCountById);
    }

    private IReadOnlyList<CategoryDto> BuildTree(
        IReadOnlyList<Category> all,
        Guid? parentId,
        Dictionary<Guid, int> itemCountById)
    {
        return all
            .Where(c => c.ParentId == parentId)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c =>
            {
                var dto = mapper.Map<CategoryDto>(c);
                dto.ItemCount = itemCountById.GetValueOrDefault(c.Id);
                dto.Children = BuildTree(all, c.Id, itemCountById);
                return dto;
            })
            .ToList();
    }
}
