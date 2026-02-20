using MediatR;

namespace Seems.Application.Identity.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid Id) : IRequest;
