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
        var (pages, total) = await pageRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        var dtos = mapper.Map<IReadOnlyList<PageDto>>(pages);
        return new PaginatedList<PageDto>(dtos, total, request.Page, request.PageSize);
    }
}
