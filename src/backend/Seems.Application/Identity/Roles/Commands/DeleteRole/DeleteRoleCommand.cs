using MediatR;

namespace Seems.Application.Identity.Roles.Commands.DeleteRole;

public record DeleteRoleCommand(Guid Id) : IRequest;
