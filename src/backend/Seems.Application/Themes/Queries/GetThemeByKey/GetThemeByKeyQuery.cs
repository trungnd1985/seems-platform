using MediatR;
using Seems.Application.Themes.Dtos;

namespace Seems.Application.Themes.Queries.GetThemeByKey;

public record GetThemeByKeyQuery(string Key) : IRequest<ThemeDto?>;
