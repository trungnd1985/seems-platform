using MediatR;
using Microsoft.AspNetCore.Identity;
using Seems.Domain.Entities.Identity;

namespace Seems.Application.Identity.Users.Commands.ResetUserPassword;

public class ResetUserPasswordHandler(UserManager<AppUser> userManager)
    : IRequestHandler<ResetUserPasswordCommand>
{
    public async Task Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString())
            ?? throw new KeyNotFoundException($"User '{request.Id}' not found.");

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
            throw new InvalidOperationException(result.Errors.First().Description);
    }
}
