using MediatR;
using Seems.Application.Modules.Dtos;

namespace Seems.Application.Modules.Commands.UpdateModule;

public record UpdateModuleCommand(
    Guid Id,
    string Name,
    string Version,
    string? PublicComponentUrl,
    string? Description,
    string? Author) : IRequest<ModuleDto>;
