using MediatR;
using Microsoft.AspNetCore.Identity;
using Seems.Application.Common.Interfaces;
using Seems.Domain.Entities.Identity;

namespace Seems.Application.Identity.Users.Commands.LockUser;

public class LockUserHandler(
    UserManager<AppUser> userManager,
    ICurrentUser currentUser)
    : IRequestHandler<LockUserCommand>
{
    public async Task Handle(LockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString())
            ?? throw new KeyNotFoundException($"User '{request.Id}' not found.");

        if (currentUser.UserId == request.Id)
            throw new InvalidOperationException("You cannot lock your own account.");

        var admins = await userManager.GetUsersInRoleAsync("Admin");
        var isAdmin = admins.Any(a => a.Id == request.Id);
        if (isAdmin && admins.Count <= 1)
            throw new InvalidOperationException("Cannot lock the last admin user.");

        await userManager.SetLockoutEnabledAsync(user, true);
        var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        if (!result.Succeeded)
            throw new InvalidOperationException(result.Errors.First().Description);
    }
}
