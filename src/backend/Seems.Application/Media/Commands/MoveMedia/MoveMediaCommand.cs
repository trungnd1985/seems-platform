using MediatR;
using Seems.Application.Media.Dtos;

namespace Seems.Application.Media.Commands.MoveMedia;

public record MoveMediaCommand(Guid Id, Guid? TargetFolderId) : IRequest<MediaDto>;
