using MediatR;

namespace Seems.Application.Identity.Users.Commands.LockUser;

public record LockUserCommand(Guid Id) : IRequest;
