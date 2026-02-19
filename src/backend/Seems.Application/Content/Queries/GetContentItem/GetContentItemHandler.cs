using AutoMapper;
using MediatR;
using Seems.Application.Content.Dtos;
using Seems.Domain.Interfaces;

namespace Seems.Application.Content.Queries.GetContentItem;

public class GetContentItemHandler(IContentRepository contentRepository, IMapper mapper)
    : IRequestHandler<GetContentItemQuery, ContentItemDto?>
{
    public async Task<ContentItemDto?> Handle(GetContentItemQuery request, CancellationToken cancellationToken)
    {
        var item = await contentRepository.GetByIdAsync(request.Id, cancellationToken);
        return item is null ? null : mapper.Map<ContentItemDto>(item);
    }
}
