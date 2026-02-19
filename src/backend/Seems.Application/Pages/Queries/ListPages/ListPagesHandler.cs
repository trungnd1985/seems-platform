using AutoMapper;
using MediatR;
using Seems.Application.Common.Models;
using Seems.Application.Pages.Dtos;
using Seems.Domain.Interfaces;

namespace Seems.Application.Pages.Queries.ListPages;

public class ListPagesHandler(IPageRepository pageRepository, IMapper mapper)
    : IRequestHandler<ListPagesQuery, PaginatedList<PageDto>>
{
    public async Task<PaginatedList<PageDto>> Handle(ListPagesQuery request, CancellationToken cancellationToken)
    {
        var pages = await pageRepository.GetAllAsync(cancellationToken);
        var dtos = mapper.Map<IReadOnlyList<PageDto>>(pages);
        var paged = dtos
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedList<PageDto>(paged, dtos.Count, request.Page, request.PageSize);
    }
}
