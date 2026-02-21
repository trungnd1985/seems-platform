using MediatR;
using Seems.Application.Templates.Dtos;

namespace Seems.Application.Templates.Commands.CreateTemplate;

public record CreateTemplateCommand(
    string Key,
    string Name,
    string ThemeKey,
    IReadOnlyList<TemplateSlotDef> Slots
) : IRequest<TemplateDto>;
