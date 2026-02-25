using AutoMapper;
using MediatR;
using Seems.Application.Pages.Dtos;
using Seems.Domain.Interfaces;

namespace Seems.Application.Pages.Queries.GetPageTree;

public class GetPageTreeHandler(IPageRepository pageRepository, IMapper mapper)
    : IRequestHandler<GetPageTreeQuery, IReadOnlyList<PageDto>>
{
    public async Task<IReadOnlyList<PageDto>> Handle(GetPageTreeQuery request, CancellationToken cancellationToken)
    {
        var pages = await pageRepository.GetAllForTreeAsync(cancellationToken);
        return mapper.Map<IReadOnlyList<PageDto>>(pages);
    }
}
