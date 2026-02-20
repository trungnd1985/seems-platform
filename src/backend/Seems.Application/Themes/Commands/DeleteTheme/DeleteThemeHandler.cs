using MediatR;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Themes.Commands.DeleteTheme;

public class DeleteThemeHandler(
    IRepository<Theme> repository,
    IRepository<Template> templateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteThemeCommand>
{
    public async Task Handle(DeleteThemeCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Theme '{request.Id}' not found.");

        var templates = await templateRepository.FindAsync(
            t => t.ThemeKey == entity.Key, cancellationToken);

        if (templates.Count > 0)
            throw new InvalidOperationException(
                $"Theme '{entity.Key}' is used by {templates.Count} template(s). Reassign or delete them before removing the theme.");

        repository.Delete(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
