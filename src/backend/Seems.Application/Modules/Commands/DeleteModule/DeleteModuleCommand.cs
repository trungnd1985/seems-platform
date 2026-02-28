using MediatR;

namespace Seems.Application.Modules.Commands.DeleteModule;

public record DeleteModuleCommand(Guid Id) : IRequest;
