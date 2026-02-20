using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Seems.Application.Identity.Roles.Dtos;
using Seems.Domain.Entities.Identity;

namespace Seems.Application.Identity.Roles.Queries.ListRoles;

public class ListRolesHandler(
    RoleManager<AppRole> roleManager,
    UserManager<AppUser> userManager)
    : IRequestHandler<ListRolesQuery, List<RoleDto>>
{
    private static readonly string[] SystemRoles = ["Admin", "Editor", "Viewer"];

    public async Task<List<RoleDto>> Handle(ListRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await roleManager.Roles
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        var result = new List<RoleDto>(roles.Count);

        foreach (var role in roles)
        {
            var users = await userManager.GetUsersInRoleAsync(role.Name!);
            result.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name!,
                Description = role.Description,
                UserCount = users.Count,
                IsSystem = SystemRoles.Contains(role.Name),
                CreatedAt = role.CreatedAt,
            });
        }

        return result;
    }
}
