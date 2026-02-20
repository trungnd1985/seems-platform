using AutoMapper;
using MediatR;
using Seems.Application.Content.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.ContentTypes.Queries.ListContentTypes;

public class ListContentTypesHandler(
    IRepository<ContentType> repository,
    IMapper mapper)
    : IRequestHandler<ListContentTypesQuery, IReadOnlyList<ContentTypeDto>>
{
    public async Task<IReadOnlyList<ContentTypeDto>> Handle(
        ListContentTypesQuery request,
        CancellationToken cancellationToken)
    {
        var items = await repository.GetAllAsync(cancellationToken);
        return mapper.Map<IReadOnlyList<ContentTypeDto>>(items);
    }
}
