using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Seems.Application.Common.Models;
using Seems.Application.Identity.Users.Dtos;
using Seems.Domain.Entities.Identity;

namespace Seems.Application.Identity.Users.Queries.ListUsers;

public class ListUsersHandler(UserManager<AppUser> userManager)
    : IRequestHandler<ListUsersQuery, PaginatedList<UserDetailDto>>
{
    public async Task<PaginatedList<UserDetailDto>> Handle(
        ListUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await userManager.Users
            .OrderBy(u => u.DisplayName)
            .ToListAsync(cancellationToken);

        var total = users.Count;
        var paged = users
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = new List<UserDetailDto>(paged.Count);
        foreach (var user in paged)
        {
            var roles = await userManager.GetRolesAsync(user);
            dtos.Add(MapToDto(user, roles));
        }

        return new PaginatedList<UserDetailDto>(dtos, total, request.Page, request.PageSize);
    }

    internal static UserDetailDto MapToDto(AppUser user, IList<string> roles) => new()
    {
        Id = user.Id,
        Email = user.Email!,
        DisplayName = user.DisplayName,
        Roles = [.. roles],
        IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
        LockoutEnd = user.LockoutEnd,
        CreatedAt = user.CreatedAt,
    };
}
