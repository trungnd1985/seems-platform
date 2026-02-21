using AutoMapper;
using MediatR;
using Seems.Application.Common.Interfaces;
using Seems.Application.Pages.Dtos;
using Seems.Domain.Interfaces;

namespace Seems.Application.Pages.Queries.GetPageBySlug;

public class GetPageBySlugHandler(
    IPageRepository pageRepository,
    IPreviewContext preview,
    IMapper mapper)
    : IRequestHandler<GetPageBySlugQuery, PageDto?>
{
    public async Task<PageDto?> Handle(GetPageBySlugQuery request, CancellationToken cancellationToken)
    {
        var page = preview.IsPreview
            ? await pageRepository.GetBySlugAsync(request.Slug, cancellationToken)
            : await pageRepository.GetPublishedBySlugAsync(request.Slug, cancellationToken);

        return page is null ? null : mapper.Map<PageDto>(page);
    }
}
