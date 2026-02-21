using MediatR;
using Seems.Application.Templates.Dtos;

namespace Seems.Application.Templates.Queries.GetTemplate;

public record GetTemplateQuery(Guid Id) : IRequest<TemplateDto>;
