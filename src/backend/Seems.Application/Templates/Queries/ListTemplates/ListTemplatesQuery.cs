using MediatR;
using Seems.Application.Templates.Dtos;

namespace Seems.Application.Templates.Queries.ListTemplates;

public record ListTemplatesQuery : IRequest<IReadOnlyList<TemplateDto>>;
