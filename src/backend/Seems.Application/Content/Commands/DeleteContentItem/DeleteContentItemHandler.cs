using MediatR;
using Seems.Domain.Interfaces;

namespace Seems.Application.Content.Commands.DeleteContentItem;

public class DeleteContentItemHandler(IContentRepository contentRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteContentItemCommand>
{
    public async Task Handle(DeleteContentItemCommand request, CancellationToken cancellationToken)
    {
        var item = await contentRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Content item '{request.Id}' not found.");

        contentRepository.Delete(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
