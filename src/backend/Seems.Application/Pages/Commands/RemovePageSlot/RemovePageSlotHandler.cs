using MediatR;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Pages.Commands.RemovePageSlot;

public class RemovePageSlotHandler(
    IPageRepository pageRepository,
    IRepository<SlotMapping> slotRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemovePageSlotCommand>
{
    public async Task Handle(RemovePageSlotCommand request, CancellationToken cancellationToken)
    {
        // Confirm the page exists before touching slots
        var page = await pageRepository.GetByIdAsync(request.PageId, cancellationToken)
            ?? throw new KeyNotFoundException($"Page '{request.PageId}' not found.");

        var mapping = await slotRepository.GetByIdAsync(request.SlotId, cancellationToken);
        if (mapping is null || mapping.PageId != request.PageId)
            throw new KeyNotFoundException($"Slot mapping '{request.SlotId}' not found on page '{request.PageId}'.");

        slotRepository.Delete(mapping);

        page.UpdatedAt = DateTime.UtcNow;
        pageRepository.Update(page);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
