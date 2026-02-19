using MediatR;
using Seems.Application.Identity.Dtos;

namespace Seems.Application.Identity.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;
