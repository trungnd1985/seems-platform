using AutoMapper;
using MediatR;
using Seems.Application.Themes.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Themes.Commands.UpdateTheme;

public class UpdateThemeHandler(
    IRepository<Theme> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdateThemeCommand, ThemeDto>
{
    public async Task<ThemeDto> Handle(UpdateThemeCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Theme '{request.Id}' not found.");

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.UpdatedAt = DateTime.UtcNow;

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<ThemeDto>(entity);
    }
}
