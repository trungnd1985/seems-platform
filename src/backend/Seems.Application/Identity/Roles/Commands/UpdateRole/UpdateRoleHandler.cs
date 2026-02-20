using MediatR;
using Microsoft.AspNetCore.Identity;
using Seems.Application.Identity.Roles.Dtos;
using Seems.Domain.Entities.Identity;

namespace Seems.Application.Identity.Roles.Commands.UpdateRole;

public class UpdateRoleHandler(
    RoleManager<AppRole> roleManager,
    UserManager<AppUser> userManager)
    : IRequestHandler<UpdateRoleCommand, RoleDto>
{
    private static readonly string[] SystemRoles = ["Admin", "Editor", "Viewer"];

    public async Task<RoleDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.Id.ToString())
            ?? throw new KeyNotFoundException($"Role '{request.Id}' not found.");

        // System roles: only description is editable, not the name
        if (SystemRoles.Contains(role.Name) && role.Name != request.Name)
            throw new InvalidOperationException($"System role '{role.Name}' cannot be renamed.");

        // Uniqueness check when name changes
        if (role.Name != request.Name)
        {
            var duplicate = await roleManager.FindByNameAsync(request.Name);
            if (duplicate is not null)
                throw new InvalidOperationException($"Role '{request.Name}' already exists.");

            role.Name = request.Name;
            role.NormalizedName = request.Name.ToUpperInvariant();
        }

        role.Description = request.Description;

        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(result.Errors.First().Description);

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
