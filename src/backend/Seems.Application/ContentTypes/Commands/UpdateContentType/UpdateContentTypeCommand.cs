using MediatR;
using Seems.Application.Content.Dtos;

namespace Seems.Application.ContentTypes.Commands.UpdateContentType;

// Key is immutable — only Name and Schema can change.
public record UpdateContentTypeCommand(
    Guid Id,
    string Name,
    string Schema
) : IRequest<ContentTypeDto>;
