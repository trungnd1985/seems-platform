using System.Text.Json;
using AutoMapper;
using MediatR;
using Seems.Application.Templates.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Templates.Commands.CreateTemplate;

public class CreateTemplateHandler(
    IRepository<Template> templateRepository,
    IRepository<Theme> themeRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateTemplateCommand, TemplateDto>
{
    public async Task<TemplateDto> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var existingKey = await templateRepository.FindAsync(t => t.Key == request.Key, cancellationToken);
        if (existingKey.Count > 0)
            throw new InvalidOperationException($"Template with key '{request.Key}' already exists.");

        var theme = await themeRepository.FindAsync(t => t.Key == request.ThemeKey, cancellationToken);
        if (theme.Count == 0)
            throw new KeyNotFoundException($"Theme '{request.ThemeKey}' does not exist.");

        var entity = new Template
        {
            Id = Guid.NewGuid(),
            Key = request.Key,
            Name = request.Name,
            ThemeKey = request.ThemeKey,
            Slots = JsonSerializer.Serialize(request.Slots),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await templateRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = mapper.Map<TemplateDto>(entity);
        dto.ThemeExists = true;
        return dto;
    }
}
