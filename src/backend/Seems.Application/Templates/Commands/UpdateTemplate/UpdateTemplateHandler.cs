using System.Text.Json;
using AutoMapper;
using MediatR;
using Seems.Application.Templates.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Templates.Commands.UpdateTemplate;

public class UpdateTemplateHandler(
    IRepository<Template> templateRepository,
    IRepository<Theme> themeRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdateTemplateCommand, TemplateDto>
{
    public async Task<TemplateDto> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = await templateRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Template '{request.Id}' not found.");

        var theme = await themeRepository.FindAsync(t => t.Key == request.ThemeKey, cancellationToken);
        if (theme.Count == 0)
            throw new KeyNotFoundException($"Theme '{request.ThemeKey}' does not exist.");

        entity.Name = request.Name;
        entity.ThemeKey = request.ThemeKey;
        entity.Slots = JsonSerializer.Serialize(request.Slots);
        entity.UpdatedAt = DateTime.UtcNow;

        templateRepository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = mapper.Map<TemplateDto>(entity);
        dto.ThemeExists = true;
        return dto;
    }
}
