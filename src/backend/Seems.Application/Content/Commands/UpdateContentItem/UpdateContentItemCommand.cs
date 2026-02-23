using MediatR;
using Seems.Application.Content.Dtos;

namespace Seems.Application.Content.Commands.UpdateContentItem;

public record UpdateContentItemCommand(
    Guid Id,
    string Data,
    string? Status,
    IEnumerable<Guid>? CategoryIds = null
) : IRequest<ContentItemDto>;
