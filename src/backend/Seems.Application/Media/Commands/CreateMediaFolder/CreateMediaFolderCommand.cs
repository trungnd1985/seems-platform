using MediatR;
using Seems.Application.Media.Dtos;

namespace Seems.Application.Media.Commands.CreateMediaFolder;

public record CreateMediaFolderCommand(string Name, Guid? ParentId) : IRequest<MediaFolderDto>;
