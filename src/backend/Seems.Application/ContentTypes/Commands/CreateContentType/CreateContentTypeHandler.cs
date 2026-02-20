using AutoMapper;
using MediatR;
using Seems.Application.Content.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.ContentTypes.Commands.CreateContentType;

public class CreateContentTypeHandler(
    IRepository<ContentType> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateContentTypeCommand, ContentTypeDto>
{
    public async Task<ContentTypeDto> Handle(CreateContentTypeCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.FindAsync(ct => ct.Key == request.Key, cancellationToken);
        if (existing.Count > 0)
            throw new InvalidOperationException($"Content type with key '{request.Key}' already exists.");

        var entity = new ContentType
        {
            Id = Guid.NewGuid(),
            Key = request.Key,
            Name = request.Name,
            Schema = request.Schema,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<ContentTypeDto>(entity);
    }
}
