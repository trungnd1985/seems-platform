using MediatR;

namespace Seems.Application.Media.Commands.DeleteMediaFolder;

public record DeleteMediaFolderCommand(Guid Id) : IRequest;
