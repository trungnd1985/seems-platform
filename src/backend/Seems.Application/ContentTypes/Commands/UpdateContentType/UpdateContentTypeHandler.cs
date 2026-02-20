using AutoMapper;
using MediatR;
using Seems.Application.Content.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.ContentTypes.Commands.UpdateContentType;

public class UpdateContentTypeHandler(
    IRepository<ContentType> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdateContentTypeCommand, ContentTypeDto>
{
    public async Task<ContentTypeDto> Handle(UpdateContentTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Content type '{request.Id}' not found.");

        entity.Name = request.Name;
        entity.Schema = request.Schema;
        entity.UpdatedAt = DateTime.UtcNow;

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<ContentTypeDto>(entity);
    }
}
