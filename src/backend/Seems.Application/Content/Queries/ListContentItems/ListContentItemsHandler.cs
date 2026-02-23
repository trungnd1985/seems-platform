using AutoMapper;
using MediatR;
using Seems.Application.Common.Models;
using Seems.Application.Content.Dtos;
using Seems.Domain.Enums;
using Seems.Domain.Interfaces;

namespace Seems.Application.Content.Queries.ListContentItems;

public class ListContentItemsHandler(IContentRepository contentRepository, IMapper mapper)
    : IRequestHandler<ListContentItemsQuery, PaginatedList<ContentItemDto>>
{
    public async Task<PaginatedList<ContentItemDto>> Handle(ListContentItemsQuery request,
        CancellationToken cancellationToken)
    {
        ContentStatus? status = null;
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<ContentStatus>(request.Status, true, out var parsed))
            status = parsed;

        var (items, total) = await contentRepository.ListAsync(
            request.ContentTypeKey,
            status,
            request.CategoryId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = mapper.Map<IReadOnlyList<ContentItemDto>>(items);
        return new PaginatedList<ContentItemDto>([.. dtos], total, request.Page, request.PageSize);
    }
}
