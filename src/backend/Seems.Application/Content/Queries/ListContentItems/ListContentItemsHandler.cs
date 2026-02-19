using AutoMapper;
using MediatR;
using Seems.Application.Common.Models;
using Seems.Application.Content.Dtos;
using Seems.Domain.Interfaces;

namespace Seems.Application.Content.Queries.ListContentItems;

public class ListContentItemsHandler(IContentRepository contentRepository, IMapper mapper)
    : IRequestHandler<ListContentItemsQuery, PaginatedList<ContentItemDto>>
{
    public async Task<PaginatedList<ContentItemDto>> Handle(ListContentItemsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await contentRepository.GetAllAsync(cancellationToken);
        var dtos = mapper.Map<IReadOnlyList<ContentItemDto>>(items);
        var paged = dtos
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedList<ContentItemDto>(paged, dtos.Count, request.Page, request.PageSize);
    }
}
