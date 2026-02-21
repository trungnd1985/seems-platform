using MediatR;
using Seems.Application.Pages.Dtos;

namespace Seems.Application.Pages.Queries.GetPageById;

public record GetPageByIdQuery(Guid Id) : IRequest<PageDto>;
