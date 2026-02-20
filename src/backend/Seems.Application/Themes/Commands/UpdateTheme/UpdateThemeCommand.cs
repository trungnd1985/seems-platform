using MediatR;
using Seems.Application.Themes.Dtos;

namespace Seems.Application.Themes.Commands.UpdateTheme;

public record UpdateThemeCommand(
    Guid Id,
    string Name,
    string? Description
) : IRequest<ThemeDto>;
