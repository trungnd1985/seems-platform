using System.Text.Json;
using MediatR;

namespace Seems.Application.Pages.Commands.UpdateSlotParameters;

public record UpdateSlotParametersCommand(
    Guid PageId,
    Guid SlotId,
    Dictionary<string, JsonElement>? Parameters
) : IRequest;
