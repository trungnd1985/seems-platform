using MediatR;
using Microsoft.AspNetCore.Identity;
using Seems.Application.Identity.Roles.Dtos;
using Seems.Domain.Entities.Identity;

namespace Seems.Application.Identity.Roles.Queries.GetRole;

public class GetRoleHandler(
    RoleManager<AppRole> roleManager,
    UserManager<AppUser> userManager)
    : IRequestHandler<GetRoleQuery, RoleDto>
{
    private static readonly string[] SystemRoles = ["Admin", "Editor", "Viewer"];

    public async Task<RoleDto> Handle(GetRoleQuery request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.Id.ToString())
            ?? throw new KeyNotFoundException($"Role '{request.Id}' not found.");

        var users = await userManager.GetUsersInRoleAsync(role.Name!);

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name!,
            Description = role.Description,
            UserCount = users.Count,
            IsSystem = SystemRoles.Contains(role.Name),
            CreatedAt = role.CreatedAt,
        };
    }
}
