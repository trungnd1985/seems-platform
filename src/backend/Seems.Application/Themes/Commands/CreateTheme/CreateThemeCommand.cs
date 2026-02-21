using MediatR;
using Seems.Application.Themes.Dtos;

namespace Seems.Application.Themes.Commands.CreateTheme;

public record CreateThemeCommand(
    string Key,
    string Name,
    string? Description,
    string? CssUrl
) : IRequest<ThemeDto>;
