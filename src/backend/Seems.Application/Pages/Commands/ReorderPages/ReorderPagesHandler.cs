using MediatR;
using Seems.Domain.Interfaces;

namespace Seems.Application.Pages.Commands.ReorderPages;

public class ReorderPagesHandler(IPageRepository pageRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<ReorderPagesCommand>
{
    public async Task Handle(ReorderPagesCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.Items)
        {
            var page = await pageRepository.GetByIdAsync(item.PageId, cancellationToken)
                ?? throw new KeyNotFoundException($"Page '{item.PageId}' not found.");

            page.SortOrder = item.SortOrder;
            page.UpdatedAt = DateTime.UtcNow;
            pageRepository.Update(page);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
