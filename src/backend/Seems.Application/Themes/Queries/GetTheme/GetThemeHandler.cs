using AutoMapper;
using MediatR;
using Seems.Application.Themes.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Themes.Queries.GetTheme;

public class GetThemeHandler(
    IRepository<Theme> repository,
    IMapper mapper)
    : IRequestHandler<GetThemeQuery, ThemeDto>
{
    public async Task<ThemeDto> Handle(GetThemeQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Theme '{request.Id}' not found.");

        return mapper.Map<ThemeDto>(entity);
    }
}
