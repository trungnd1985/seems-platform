using MediatR;
using Microsoft.AspNetCore.Identity;
using Seems.Domain.Entities.Identity;

namespace Seems.Application.Identity.Users.Commands.UnlockUser;

public class UnlockUserHandler(UserManager<AppUser> userManager)
    : IRequestHandler<UnlockUserCommand>
{
    public async Task Handle(UnlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString())
            ?? throw new KeyNotFoundException($"User '{request.Id}' not found.");

        await userManager.SetLockoutEndDateAsync(user, null);
        var result = await userManager.ResetAccessFailedCountAsync(user);

        if (!result.Succeeded)
            throw new InvalidOperationException(result.Errors.First().Description);
    }
}
