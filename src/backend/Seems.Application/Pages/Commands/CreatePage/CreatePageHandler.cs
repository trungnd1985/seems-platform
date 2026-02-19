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
        var page = new Page
        {
            Id = Guid.NewGuid(),
            Slug = request.Slug,
            Title = request.Title,
            TemplateKey = request.TemplateKey,
            ThemeKey = request.ThemeKey,
            Seo = request.Seo ?? new SeoMeta { Title = request.Title },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await pageRepository.AddAsync(page, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<PageDto>(page);
    }
}
