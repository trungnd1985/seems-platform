using MediatR;

namespace Seems.Application.ContentTypes.Commands.DeleteContentType;

public record DeleteContentTypeCommand(Guid Id) : IRequest;
