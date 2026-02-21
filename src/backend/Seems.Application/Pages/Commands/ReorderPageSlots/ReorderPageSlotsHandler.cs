using MediatR;
using Seems.Domain.Interfaces;

namespace Seems.Application.Pages.Commands.ReorderPageSlots;

public class ReorderPageSlotsHandler(IPageRepository pageRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<ReorderPageSlotsCommand>
{
    public async Task Handle(ReorderPageSlotsCommand request, CancellationToken cancellationToken)
    {
        var page = await pageRepository.GetWithSlotsAsync(request.PageId, cancellationToken)
            ?? throw new KeyNotFoundException($"Page '{request.PageId}' not found.");

        var slotMap = page.Slots.ToDictionary(s => s.Id);

        foreach (var item in request.Items)
        {
            if (!slotMap.TryGetValue(item.SlotId, out var mapping))
                throw new KeyNotFoundException($"Slot mapping '{item.SlotId}' not found on page '{request.PageId}'.");

            mapping.Order = item.Order;
        }

        page.UpdatedAt = DateTime.UtcNow;
        pageRepository.Update(page);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
