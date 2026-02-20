using MediatR;

namespace Seems.Application.Identity.Users.Commands.ResetUserPassword;

public record ResetUserPasswordCommand(Guid Id, string NewPassword) : IRequest;
