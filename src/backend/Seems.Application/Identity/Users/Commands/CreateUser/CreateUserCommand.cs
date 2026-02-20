using MediatR;
using Seems.Application.Identity.Users.Dtos;

namespace Seems.Application.Identity.Users.Commands.CreateUser;

public record CreateUserCommand(
    string Email,
    string DisplayName,
    string Password,
    List<string> Roles
) : IRequest<UserDetailDto>;
