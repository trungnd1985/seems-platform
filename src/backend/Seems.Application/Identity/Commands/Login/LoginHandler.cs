using MediatR;
using Microsoft.AspNetCore.Identity;
using Seems.Application.Common.Interfaces;
using Seems.Application.Identity.Dtos;
using Seems.Domain.Entities.Identity;

namespace Seems.Application.Identity.Commands.Login;

public class LoginHandler(
    UserManager<AppUser> userManager,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email)
                   ?? throw new UnauthorizedAccessException("Invalid credentials.");

        var valid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!valid)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var roles = await userManager.GetRolesAsync(user);
        var token = await jwtTokenService.GenerateTokenAsync(user);

        return new LoginResponse
        {
            AccessToken = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                DisplayName = user.DisplayName,
                Roles = roles.ToList(),
            },
        };
    }
}
