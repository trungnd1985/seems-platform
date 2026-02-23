using AutoMapper;
using MediatR;
using Seems.Application.Categories.Dtos;
using Seems.Domain.Interfaces;

namespace Seems.Application.Categories.Commands.UpdateCategory;

public class UpdateCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category '{request.Id}' not found.");

        var slug = BuildSlug(request.Slug ?? request.Name);

        // Check slug uniqueness at the same level, excluding self
        var duplicate = await categoryRepository.FindAsync(
            c => c.ContentTypeKey == category.ContentTypeKey
              && c.ParentId == request.ParentId
              && c.Slug == slug
              && c.Id != request.Id,
            cancellationToken);

        if (duplicate.Count > 0)
            throw new InvalidOperationException($"A category with slug '{slug}' already exists at this level.");

        category.Name = request.Name;
        category.Slug = slug;
        category.Description = request.Description;
        category.ParentId = request.ParentId;
        category.SortOrder = request.SortOrder;
        category.UpdatedAt = DateTime.UtcNow;

        categoryRepository.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<CategoryDto>(category);
    }

    private static string BuildSlug(string source)
    {
        var slug = source.Trim().ToLowerInvariant();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[\s-]+", "-");
        return slug.Trim('-');
    }
}
