using MediatR;
using Seems.Application.Common.Models;
using Seems.Application.Media.Dtos;

namespace Seems.Application.Media.Queries.ListMedia;

public record ListMediaQuery(Guid? FolderId, int Page, int PageSize) : IRequest<PaginatedList<MediaDto>>;
