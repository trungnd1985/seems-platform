using MediatR;
using Seems.Domain.Interfaces;

namespace Seems.Application.Pages.Commands.DeletePage;

public class DeletePageHandler(IPageRepository pageRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePageCommand>
{
    public async Task Handle(DeletePageCommand request, CancellationToken cancellationToken)
    {
        var page = await pageRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Page '{request.Id}' not found.");

        if (await pageRepository.HasChildrenAsync(request.Id, cancellationToken))
            throw new InvalidOperationException("Cannot delete a page that has child pages. Re-parent or delete the children first.");

        pageRepository.Delete(page);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
