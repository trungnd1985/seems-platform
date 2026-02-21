using AutoMapper;
using MediatR;
using Seems.Application.Templates.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Templates.Queries.GetTemplate;

public class GetTemplateHandler(
    IRepository<Template> templateRepository,
    IRepository<Theme> themeRepository,
    IMapper mapper)
    : IRequestHandler<GetTemplateQuery, TemplateDto>
{
    public async Task<TemplateDto> Handle(GetTemplateQuery request, CancellationToken cancellationToken)
    {
        var entity = await templateRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Template '{request.Id}' not found.");

        var dto = mapper.Map<TemplateDto>(entity);

        var themes = await themeRepository.FindAsync(t => t.Key == entity.ThemeKey, cancellationToken);
        dto.ThemeExists = themes.Count > 0;

        return dto;
    }
}
