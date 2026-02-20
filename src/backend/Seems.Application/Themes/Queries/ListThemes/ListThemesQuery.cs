using MediatR;
using Seems.Application.Themes.Dtos;

namespace Seems.Application.Themes.Queries.ListThemes;

public record ListThemesQuery : IRequest<IReadOnlyList<ThemeDto>>;
