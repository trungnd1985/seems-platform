using AutoMapper;
using MediatR;
using Seems.Application.Templates.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Templates.Queries.ListTemplates;

public class ListTemplatesHandler(IRepository<Template> templateRepository, IMapper mapper)
    : IRequestHandler<ListTemplatesQuery, IReadOnlyList<TemplateDto>>
{
    public async Task<IReadOnlyList<TemplateDto>> Handle(ListTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var templates = await templateRepository.GetAllAsync(cancellationToken);
        return mapper.Map<IReadOnlyList<TemplateDto>>(templates);
    }
}
