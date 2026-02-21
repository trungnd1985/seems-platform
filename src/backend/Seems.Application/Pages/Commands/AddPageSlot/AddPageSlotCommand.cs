using MediatR;
using Seems.Application.Pages.Dtos;
using Seems.Domain.Enums;

namespace Seems.Application.Pages.Commands.AddPageSlot;

public record AddPageSlotCommand(
    Guid PageId,
    string SlotKey,
    SlotTargetType TargetType,
    string TargetId
) : IRequest<SlotMappingDto>;
