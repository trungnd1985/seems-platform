using MediatR;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.ContentTypes.Commands.DeleteContentType;

public class DeleteContentTypeHandler(
    IRepository<ContentType> repository,
    IContentRepository contentRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteContentTypeCommand>
{
    public async Task Handle(DeleteContentTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Content type '{request.Id}' not found.");

        var items = await contentRepository.GetByContentTypeKeyAsync(entity.Key, cancellationToken);
        if (items.Count > 0)
            throw new InvalidOperationException(
                $"Content type '{entity.Key}' has {items.Count} content item(s). Remove all items before deleting the type.");

        repository.Delete(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
