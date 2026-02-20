using MediatR;
using Seems.Application.Common.Models;
using Seems.Application.Identity.Users.Dtos;

namespace Seems.Application.Identity.Users.Queries.ListUsers;

public record ListUsersQuery(int Page = 1, int PageSize = 20)
    : IRequest<PaginatedList<UserDetailDto>>;
