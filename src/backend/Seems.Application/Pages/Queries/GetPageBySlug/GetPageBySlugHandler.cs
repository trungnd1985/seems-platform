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
        var result = await pageRepository.ResolveByPathAsync(
            request.Slug, publishedOnly: !preview.IsPreview, cancellationToken);

        if (result is null) return null;

        var dto = mapper.Map<PageDto>(result.Value.Page);
        if (result.Value.UrlParams.Count > 0)
            dto.UrlParameters = result.Value.UrlParams;

        return dto;
    }
}
