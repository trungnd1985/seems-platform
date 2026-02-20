using MediatR;

namespace Seems.Application.Identity.Users.Commands.UnlockUser;

public record UnlockUserCommand(Guid Id) : IRequest;
