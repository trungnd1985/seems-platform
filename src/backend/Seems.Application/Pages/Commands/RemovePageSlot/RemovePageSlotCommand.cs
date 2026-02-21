using MediatR;

namespace Seems.Application.Pages.Commands.RemovePageSlot;

public record RemovePageSlotCommand(Guid PageId, Guid SlotId) : IRequest;
