using AutoMapper;
using MediatR;
using Seems.Application.Pages.Dtos;
using Seems.Domain.Interfaces;

namespace Seems.Application.Pages.Queries.GetDefaultPage;

public class GetDefaultPageHandler(IPageRepository pageRepository, IMapper mapper)
    : IRequestHandler<GetDefaultPageQuery, PageDto?>
{
    public async Task<PageDto?> Handle(GetDefaultPageQuery request, CancellationToken cancellationToken)
    {
        var page = await pageRepository.GetDefaultAsync(cancellationToken);
        return page is null ? null : mapper.Map<PageDto>(page);
    }
}
