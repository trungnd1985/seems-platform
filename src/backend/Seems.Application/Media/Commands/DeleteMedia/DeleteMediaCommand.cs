using MediatR;

namespace Seems.Application.Media.Commands.DeleteMedia;

public record DeleteMediaCommand(Guid Id) : IRequest;
