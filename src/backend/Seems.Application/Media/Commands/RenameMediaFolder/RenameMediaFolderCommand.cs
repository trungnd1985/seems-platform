using MediatR;
using Seems.Application.Media.Dtos;

namespace Seems.Application.Media.Commands.RenameMediaFolder;

public record RenameMediaFolderCommand(Guid Id, string Name) : IRequest<MediaFolderDto>;
