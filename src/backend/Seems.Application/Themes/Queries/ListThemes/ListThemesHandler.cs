using AutoMapper;
using MediatR;
using Seems.Application.Themes.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Themes.Queries.ListThemes;

public class ListThemesHandler(
    IRepository<Theme> repository,
    IMapper mapper)
    : IRequestHandler<ListThemesQuery, IReadOnlyList<ThemeDto>>
{
    public async Task<IReadOnlyList<ThemeDto>> Handle(
        ListThemesQuery request,
        CancellationToken cancellationToken)
    {
        var items = await repository.GetAllAsync(cancellationToken);
        return mapper.Map<IReadOnlyList<ThemeDto>>(items);
    }
}
