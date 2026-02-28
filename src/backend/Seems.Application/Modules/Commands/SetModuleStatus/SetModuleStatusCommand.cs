using MediatR;
using Seems.Application.Modules.Dtos;

namespace Seems.Application.Modules.Commands.SetModuleStatus;

public record SetModuleStatusCommand(Guid Id, string Status) : IRequest<ModuleDto>;
