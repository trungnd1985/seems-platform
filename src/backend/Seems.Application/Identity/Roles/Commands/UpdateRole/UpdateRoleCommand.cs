using MediatR;
using Seems.Application.Identity.Roles.Dtos;

namespace Seems.Application.Identity.Roles.Commands.UpdateRole;

public record UpdateRoleCommand(Guid Id, string Name, string? Description) : IRequest<RoleDto>;
