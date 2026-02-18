using MediatR;
using Seems.Application.Pages.Dtos;
using Seems.Domain.ValueObjects;

namespace Seems.Application.Pages.Commands.CreatePage;

public record CreatePageCommand(
    string Slug,
    string Title,
    string TemplateKey,
    string? ThemeKey,
    SeoMeta? Seo
) : IRequest<PageDto>;
