using MediatR;
using Seems.Application.Content.Dtos;

namespace Seems.Application.ContentTypes.Commands.CreateContentType;

public record CreateContentTypeCommand(
    string Key,
    string Name,
    string Schema
) : IRequest<ContentTypeDto>;
