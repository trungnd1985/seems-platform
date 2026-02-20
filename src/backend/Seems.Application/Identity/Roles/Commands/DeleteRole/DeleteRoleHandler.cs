using MediatR;
using Microsoft.AspNetCore.Identity;
using Seems.Domain.Entities.Identity;

namespace Seems.Application.Identity.Roles.Commands.DeleteRole;

public class DeleteRoleHandler(
    RoleManager<AppRole> roleManager,
    UserManager<AppUser> userManager)
    : IRequestHandler<DeleteRoleCommand>
{
    private static readonly string[] SystemRoles = ["Admin", "Editor", "Viewer"];

    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.Id.ToString())
            ?? throw new KeyNotFoundException($"Role '{request.Id}' not found.");

        if (SystemRoles.Contains(role.Name))
            throw new InvalidOperationException($"System role '{role.Name}' cannot be deleted.");

        var users = await userManager.GetUsersInRoleAsync(role.Name!);
        if (users.Count > 0)
            throw new InvalidOperationException(
                $"Role '{role.Name}' has {users.Count} assigned user(s) and cannot be deleted.");

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(result.Errors.First().Description);
    }
}
