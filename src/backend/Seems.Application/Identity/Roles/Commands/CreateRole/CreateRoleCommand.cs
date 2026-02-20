using MediatR;
using Seems.Application.Identity.Roles.Dtos;

namespace Seems.Application.Identity.Roles.Commands.CreateRole;

public record CreateRoleCommand(string Name, string? Description) : IRequest<RoleDto>;
