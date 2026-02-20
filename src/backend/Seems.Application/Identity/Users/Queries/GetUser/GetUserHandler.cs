using MediatR;
using Microsoft.AspNetCore.Identity;
using Seems.Application.Identity.Users.Dtos;
using Seems.Application.Identity.Users.Queries.ListUsers;
using Seems.Domain.Entities.Identity;

namespace Seems.Application.Identity.Users.Queries.GetUser;

public class GetUserHandler(UserManager<AppUser> userManager)
    : IRequestHandler<GetUserQuery, UserDetailDto>
{
    public async Task<UserDetailDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString())
            ?? throw new KeyNotFoundException($"User '{request.Id}' not found.");

        var roles = await userManager.GetRolesAsync(user);
        return ListUsersHandler.MapToDto(user, roles);
    }
}
