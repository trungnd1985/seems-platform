using MediatR;
using Seems.Application.Pages.Dtos;

namespace Seems.Application.Pages.Queries.GetPageBySlug;

public record GetPageBySlugQuery(string Slug) : IRequest<PageDto?>;
