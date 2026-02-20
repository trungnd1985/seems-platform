using MediatR;
using Microsoft.AspNetCore.Identity;
using Seems.Application.Identity.Users.Dtos;
using Seems.Application.Identity.Users.Queries.ListUsers;
using Seems.Domain.Entities.Identity;

namespace Seems.Application.Identity.Users.Commands.UpdateUser;

public class UpdateUserHandler(
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager)
    : IRequestHandler<UpdateUserCommand, UserDetailDto>
{
    public async Task<UserDetailDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString())
            ?? throw new KeyNotFoundException($"User '{request.Id}' not found.");

        // Validate email uniqueness if changed
        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var duplicate = await userManager.FindByEmailAsync(request.Email);
            if (duplicate is not null)
                throw new InvalidOperationException($"Email '{request.Email}' is already in use.");
        }

        // Validate roles exist
        foreach (var roleName in request.Roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                throw new InvalidOperationException($"Role '{roleName}' does not exist.");
        }

        // Guard: cannot remove Admin role if this is the last Admin
        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Contains("Admin") && !request.Roles.Contains("Admin"))
        {
            var admins = await userManager.GetUsersInRoleAsync("Admin");
            if (admins.Count <= 1)
                throw new InvalidOperationException("Cannot remove Admin role from the last admin user.");
        }

        user.Email = request.Email;
        user.UserName = request.Email;
        user.NormalizedEmail = request.Email.ToUpperInvariant();
        user.NormalizedUserName = request.Email.ToUpperInvariant();
        user.DisplayName = request.DisplayName;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new InvalidOperationException(updateResult.Errors.First().Description);

        // Sync roles: remove old, add new
        var toRemove = currentRoles.Except(request.Roles).ToList();
        var toAdd = request.Roles.Except(currentRoles).ToList();

        if (toRemove.Count > 0)
            await userManager.RemoveFromRolesAsync(user, toRemove);

        if (toAdd.Count > 0)
            await userManager.AddToRolesAsync(user, toAdd);

        var roles = await userManager.GetRolesAsync(user);
        return ListUsersHandler.MapToDto(user, roles);
    }
}
