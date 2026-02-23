using MediatR;

namespace Seems.Application.Content.Commands.DeleteContentItem;

public record DeleteContentItemCommand(Guid Id) : IRequest;
