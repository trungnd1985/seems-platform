using MediatR;
using Seems.Application.Identity.Users.Dtos;

namespace Seems.Application.Identity.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid Id,
    string Email,
    string DisplayName,
    List<string> Roles
) : IRequest<UserDetailDto>;
