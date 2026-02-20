using MediatR;

namespace Seems.Application.Themes.Commands.DeleteTheme;

public record DeleteThemeCommand(Guid Id) : IRequest;
