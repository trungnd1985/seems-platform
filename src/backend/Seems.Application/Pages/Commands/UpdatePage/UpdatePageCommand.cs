using MediatR;
using Seems.Application.Pages.Dtos;
using Seems.Domain.ValueObjects;

namespace Seems.Application.Pages.Commands.UpdatePage;

public record UpdatePageCommand(
    Guid Id,
    string Slug,
    string Title,
    string TemplateKey,
    string? ThemeKey,
    SeoMeta? Seo,
    bool IsDefault = false,
    Guid? ParentId = null,
    int SortOrder = 0,
    bool ShowInNavigation = true
) : IRequest<PageDto>;
