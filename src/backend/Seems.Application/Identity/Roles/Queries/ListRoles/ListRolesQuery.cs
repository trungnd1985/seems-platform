using MediatR;
using Seems.Application.Identity.Roles.Dtos;

namespace Seems.Application.Identity.Roles.Queries.ListRoles;

public record ListRolesQuery : IRequest<List<RoleDto>>;
