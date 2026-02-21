using AutoMapper;
using MediatR;
using Seems.Application.Common.Interfaces;
using Seems.Application.Content.Dtos;
using Seems.Domain.Enums;
using Seems.Domain.Interfaces;

namespace Seems.Application.Content.Queries.GetContentItem;

public class GetContentItemHandler(
    IContentRepository contentRepository,
    IPreviewContext preview,
    IMapper mapper)
    : IRequestHandler<GetContentItemQuery, ContentItemDto?>
{
    public async Task<ContentItemDto?> Handle(GetContentItemQuery request, CancellationToken cancellationToken)
    {
        if (preview.IsPreview)
        {
            var item = await contentRepository.GetByIdAsync(request.Id, cancellationToken);
            return item is null ? null : mapper.Map<ContentItemDto>(item);
        }

        var results = await contentRepository.FindAsync(
            c => c.Id == request.Id && c.Status == ContentStatus.Published,
            cancellationToken);

        var published = results.FirstOrDefault();
        return published is null ? null : mapper.Map<ContentItemDto>(published);
    }
}
