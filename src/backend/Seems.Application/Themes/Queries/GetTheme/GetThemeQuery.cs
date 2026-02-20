using MediatR;
using Seems.Application.Themes.Dtos;

namespace Seems.Application.Themes.Queries.GetTheme;

public record GetThemeQuery(Guid Id) : IRequest<ThemeDto>;
