using MediatR;
using Microsoft.AspNetCore.Identity;
using Seems.Application.Common.Interfaces;
using Seems.Domain.Entities.Identity;

namespace Seems.Application.Identity.Users.Commands.DeleteUser;

public class DeleteUserHandler(
    UserManager<AppUser> userManager,
    ICurrentUser currentUser)
    : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString())
            ?? throw new KeyNotFoundException($"User '{request.Id}' not found.");

        if (currentUser.UserId == request.Id)
            throw new InvalidOperationException("You cannot delete your own account.");

        var admins = await userManager.GetUsersInRoleAsync("Admin");
        var isAdmin = admins.Any(a => a.Id == request.Id);
        if (isAdmin && admins.Count <= 1)
            throw new InvalidOperationException("Cannot delete the last admin user.");

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(result.Errors.First().Description);
    }
}
