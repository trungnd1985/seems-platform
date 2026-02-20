using MediatR;
using Seems.Application.Identity.Users.Dtos;

namespace Seems.Application.Identity.Users.Queries.GetUser;

public record GetUserQuery(Guid Id) : IRequest<UserDetailDto>;
