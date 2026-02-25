using AutoMapper;
using MediatR;
using Seems.Application.Pages.Dtos;
using Seems.Domain.Interfaces;
using Seems.Domain.ValueObjects;

namespace Seems.Application.Pages.Commands.UpdatePage;

public class UpdatePageHandler(IPageRepository pageRepository, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<UpdatePageCommand, PageDto>
{
    public async Task<PageDto> Handle(UpdatePageCommand request, CancellationToken cancellationToken)
    {
        var page = await pageRepository.GetWithSlotsAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Page '{request.Id}' not found.");

        // Default pages have no URL path — preserve existing slug/path
        if (!request.IsDefault)
        {
            // Cycle detection: cannot make a page its own descendant
            if (request.ParentId.HasValue)
            {
                if (request.ParentId.Value == request.Id)
                    throw new InvalidOperationException("A page cannot be its own parent.");

                var descendants = await pageRepository.GetDescendantsAsync(request.Id, cancellationToken);
                if (descendants.Any(d => d.Id == request.ParentId.Value))
                    throw new InvalidOperationException("Cannot set a descendant page as the parent (circular reference).");
            }

            // Compute new path
            string newPath;
            if (request.ParentId.HasValue)
            {
                var parent = await pageRepository.GetByIdAsync(request.ParentId.Value, cancellationToken)
                    ?? throw new KeyNotFoundException($"Parent page '{request.ParentId}' not found.");
                newPath = parent.Path + "/" + request.Slug;
            }
            else
            {
                newPath = request.Slug;
            }

            bool pathChanged = page.Path != newPath;

            if (pathChanged)
            {
                // Ensure the new path is not already taken by another page
                var existing = await pageRepository.GetBySlugAsync(newPath, cancellationToken);
                if (existing is not null && existing.Id != page.Id)
                    throw new InvalidOperationException($"Path '{newPath}' is already in use.");

                // Update paths of all descendants by replacing the old path prefix
                var oldPathPrefix = page.Path + "/";
                var descendants = await pageRepository.GetDescendantsAsync(page.Id, cancellationToken);
                foreach (var descendant in descendants)
                {
                    descendant.Path = newPath + "/" + descendant.Path[oldPathPrefix.Length..];
                    descendant.UpdatedAt = DateTime.UtcNow;
                    pageRepository.Update(descendant);
                }
            }

            page.Slug = request.Slug;
            page.Path = newPath;
            page.ParentId = request.ParentId;
        }

        page.SortOrder = request.SortOrder;
        page.ShowInNavigation = request.ShowInNavigation;
        page.Title = request.Title;
        page.TemplateKey = request.TemplateKey;
        page.ThemeKey = request.ThemeKey;
        page.Seo = request.Seo ?? new SeoMeta { Title = request.Title };
        page.UpdatedAt = DateTime.UtcNow;

        pageRepository.Update(page);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Re-fetch with parent navigation for AutoMapper
        var saved = await pageRepository.GetWithSlotsAsync(page.Id, cancellationToken) ?? page;
        return mapper.Map<PageDto>(saved);
    }
}
