using MediatR;
using Microsoft.AspNetCore.Identity;
using Seems.Application.Identity.Users.Dtos;
using Seems.Application.Identity.Users.Queries.ListUsers;
using Seems.Domain.Entities.Identity;

namespace Seems.Application.Identity.Users.Commands.CreateUser;

public class CreateUserHandler(
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager)
    : IRequestHandler<CreateUserCommand, UserDetailDto>
{
    public async Task<UserDetailDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException($"Email '{request.Email}' is already in use.");

        foreach (var roleName in request.Roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                throw new InvalidOperationException($"Role '{roleName}' does not exist.");
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            EmailConfirmed = true,
            LockoutEnabled = true,
            CreatedAt = DateTime.UtcNow,
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            throw new InvalidOperationException(createResult.Errors.First().Description);

        if (request.Roles.Count > 0)
        {
            var rolesResult = await userManager.AddToRolesAsync(user, request.Roles);
            if (!rolesResult.Succeeded)
                throw new InvalidOperationException(rolesResult.Errors.First().Description);
        }

        var roles = await userManager.GetRolesAsync(user);
        return ListUsersHandler.MapToDto(user, roles);
    }
}
