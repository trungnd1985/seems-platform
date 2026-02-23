using AutoMapper;
using MediatR;
using Seems.Application.Categories.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Categories.Commands.CreateCategory;

public class CreateCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var slug = BuildSlug(request.Slug ?? request.Name);

        var duplicate = await categoryRepository.FindAsync(
            c => c.ContentTypeKey == request.ContentTypeKey
              && c.ParentId == request.ParentId
              && c.Slug == slug,
            cancellationToken);

        if (duplicate.Count > 0)
            throw new InvalidOperationException($"A category with slug '{slug}' already exists at this level.");

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = slug,
            Description = request.Description,
            ContentTypeKey = request.ContentTypeKey,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await categoryRepository.AddAsync(category, cancellationToken);
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
