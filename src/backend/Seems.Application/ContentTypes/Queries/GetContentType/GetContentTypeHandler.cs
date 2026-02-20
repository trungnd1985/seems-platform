using AutoMapper;
using MediatR;
using Seems.Application.Content.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.ContentTypes.Queries.GetContentType;

public class GetContentTypeHandler(
    IRepository<ContentType> repository,
    IMapper mapper)
    : IRequestHandler<GetContentTypeQuery, ContentTypeDto>
{
    public async Task<ContentTypeDto> Handle(GetContentTypeQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Content type '{request.Id}' not found.");

        return mapper.Map<ContentTypeDto>(entity);
    }
}
