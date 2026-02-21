using MediatR;
using Seems.Application.Pages.Dtos;
using Seems.Domain.Enums;

namespace Seems.Application.Pages.Commands.UpdatePageStatus;

public record UpdatePageStatusCommand(Guid Id, ContentStatus Status) : IRequest<PageDto>;
