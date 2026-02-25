using MediatR;
using Seems.Application.Pages.Dtos;

namespace Seems.Application.Pages.Queries.GetPageTree;

public record GetPageTreeQuery : IRequest<IReadOnlyList<PageDto>>;
