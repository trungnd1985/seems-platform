using MediatR;

namespace Seems.Application.Pages.Commands.ReorderPageSlots;

public record SlotOrderItem(Guid SlotId, int Order);

public record ReorderPageSlotsCommand(
    Guid PageId,
    IReadOnlyList<SlotOrderItem> Items
) : IRequest;
