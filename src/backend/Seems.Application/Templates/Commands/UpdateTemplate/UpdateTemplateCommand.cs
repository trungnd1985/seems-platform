using MediatR;
using Seems.Application.Templates.Dtos;

namespace Seems.Application.Templates.Commands.UpdateTemplate;

public record UpdateTemplateCommand(
    Guid Id,
    string Name,
    string ThemeKey,
    IReadOnlyList<TemplateSlotDef> Slots
) : IRequest<TemplateDto>;
