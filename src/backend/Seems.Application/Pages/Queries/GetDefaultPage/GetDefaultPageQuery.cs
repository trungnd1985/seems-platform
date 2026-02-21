using MediatR;
using Seems.Application.Pages.Dtos;

namespace Seems.Application.Pages.Queries.GetDefaultPage;

public record GetDefaultPageQuery : IRequest<PageDto?>;
