using AutoMapper;
using MediatR;
using Seems.Application.Themes.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Themes.Queries.GetThemeByKey;

public class GetThemeByKeyHandler(
    IRepository<Theme> repository,
    IMapper mapper)
    : IRequestHandler<GetThemeByKeyQuery, ThemeDto?>
{
    public async Task<ThemeDto?> Handle(GetThemeByKeyQuery request, CancellationToken cancellationToken)
    {
        var results = await repository.FindAsync(t => t.Key == request.Key, cancellationToken);
        var entity = results.FirstOrDefault();
        return entity is null ? null : mapper.Map<ThemeDto>(entity);
    }
}
