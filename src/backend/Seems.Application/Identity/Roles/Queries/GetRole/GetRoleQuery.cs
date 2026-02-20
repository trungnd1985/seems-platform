using MediatR;
using Seems.Application.Identity.Roles.Dtos;

namespace Seems.Application.Identity.Roles.Queries.GetRole;

public record GetRoleQuery(Guid Id) : IRequest<RoleDto>;
