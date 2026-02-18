using MediatR;
using Seems.Application.Common.Models;
using Seems.Application.Pages.Dtos;

namespace Seems.Application.Pages.Queries.ListPages;

public record ListPagesQuery(int Page = 1, int PageSize = 20) : IRequest<PaginatedList<PageDto>>;
