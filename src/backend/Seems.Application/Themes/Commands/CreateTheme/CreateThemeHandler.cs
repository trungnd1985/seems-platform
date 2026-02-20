using AutoMapper;
using MediatR;
using Seems.Application.Themes.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Themes.Commands.CreateTheme;

public class CreateThemeHandler(
    IRepository<Theme> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateThemeCommand, ThemeDto>
{
    public async Task<ThemeDto> Handle(CreateThemeCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.FindAsync(t => t.Key == request.Key, cancellationToken);
        if (existing.Count > 0)
            throw new InvalidOperationException($"Theme with key '{request.Key}' already exists.");

        var entity = new Theme
        {
            Id = Guid.NewGuid(),
            Key = request.Key,
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<ThemeDto>(entity);
    }
}
