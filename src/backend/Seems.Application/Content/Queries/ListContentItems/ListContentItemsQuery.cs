using MediatR;
using Seems.Application.Common.Models;
using Seems.Application.Content.Dtos;

namespace Seems.Application.Content.Queries.ListContentItems;

public record ListContentItemsQuery(
    int Page = 1,
    int PageSize = 20,
    string? ContentTypeKey = null,
    string? Status = null,
    Guid? CategoryId = null
) : IRequest<PaginatedList<ContentItemDto>>;
