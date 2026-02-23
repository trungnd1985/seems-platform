using MediatR;
using Seems.Application.Content.Dtos;

namespace Seems.Application.Content.Commands.CreateContentItem;

public record CreateContentItemCommand(
    string ContentTypeKey,
    string Data,
    IEnumerable<Guid>? CategoryIds = null
) : IRequest<ContentItemDto>;
