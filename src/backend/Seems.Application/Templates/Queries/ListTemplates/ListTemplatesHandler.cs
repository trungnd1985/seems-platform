using AutoMapper;
using MediatR;
using Seems.Application.Templates.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Templates.Queries.ListTemplates;

public class ListTemplatesHandler(
    IRepository<Template> templateRepository,
    IRepository<Theme> themeRepository,
    IMapper mapper)
    : IRequestHandler<ListTemplatesQuery, IReadOnlyList<TemplateDto>>
{
    public async Task<IReadOnlyList<TemplateDto>> Handle(
        ListTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var templates = await templateRepository.GetAllAsync(cancellationToken);
        var themes = await themeRepository.GetAllAsync(cancellationToken);
        var themeKeys = themes.Select(t => t.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var dtos = mapper.Map<List<TemplateDto>>(templates);
        foreach (var dto in dtos)
            dto.ThemeExists = themeKeys.Contains(dto.ThemeKey);

        return dtos;
    }
}
