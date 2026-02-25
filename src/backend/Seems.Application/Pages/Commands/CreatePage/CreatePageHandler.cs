using AutoMapper;
using MediatR;
using Seems.Application.Pages.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;
using Seems.Domain.ValueObjects;

namespace Seems.Application.Pages.Commands.CreatePage;

public class CreatePageHandler(IPageRepository pageRepository, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<CreatePageCommand, PageDto>
{
    public async Task<PageDto> Handle(CreatePageCommand request, CancellationToken cancellationToken)
    {
        string path;

        if (request.ParentId.HasValue)
        {
            var parent = await pageRepository.GetByIdAsync(request.ParentId.Value, cancellationToken)
                ?? throw new KeyNotFoundException($"Parent page '{request.ParentId}' not found.");
            path = parent.Path + "/" + request.Slug;
        }
        else
        {
            path = request.Slug;
        }

        var existing = await pageRepository.GetBySlugAsync(path, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"Path '{path}' is already in use.");

        var page = new Page
        {
            Id = Guid.NewGuid(),
            ParentId = request.ParentId,
            Slug = request.Slug,
            Path = path,
            SortOrder = request.SortOrder,
            Title = request.Title,
            TemplateKey = request.TemplateKey,
            ThemeKey = request.ThemeKey,
            Seo = request.Seo ?? new SeoMeta { Title = request.Title },
            ShowInNavigation = request.ShowInNavigation,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await pageRepository.AddAsync(page, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Re-fetch with parent navigation for AutoMapper
        var saved = await pageRepository.GetWithSlotsAsync(page.Id, cancellationToken) ?? page;
        return mapper.Map<PageDto>(saved);
    }
}
