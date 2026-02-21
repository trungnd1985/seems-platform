using AutoMapper;
using MediatR;
using Seems.Application.Pages.Dtos;
using Seems.Domain.Interfaces;

namespace Seems.Application.Pages.Queries.GetPageById;

public class GetPageByIdHandler(IPageRepository pageRepository, IMapper mapper)
    : IRequestHandler<GetPageByIdQuery, PageDto>
{
    public async Task<PageDto> Handle(GetPageByIdQuery request, CancellationToken cancellationToken)
    {
        var page = await pageRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Page '{request.Id}' not found.");

        return mapper.Map<PageDto>(page);
    }
}
