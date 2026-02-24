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
        var page = await pageRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Page '{request.Id}' not found.");

        // Default pages are resolved by the IsDefault flag — preserve their existing slug
        if (!request.IsDefault)
        {
            if (page.Slug != request.Slug)
            {
                var existing = await pageRepository.GetBySlugAsync(request.Slug, cancellationToken);
                if (existing is not null)
                    throw new InvalidOperationException($"Slug '{request.Slug}' is already in use.");
            }

            page.Slug = request.Slug;
        }
        page.Title = request.Title;
        page.TemplateKey = request.TemplateKey;
        page.ThemeKey = request.ThemeKey;
        page.Seo = request.Seo ?? new SeoMeta { Title = request.Title };
        page.UpdatedAt = DateTime.UtcNow;

        pageRepository.Update(page);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<PageDto>(page);
    }
}
