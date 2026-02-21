using MediatR;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Templates.Commands.DeleteTemplate;

public class DeleteTemplateHandler(
    IRepository<Template> templateRepository,
    IRepository<Page> pageRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteTemplateCommand>
{
    public async Task Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = await templateRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Template '{request.Id}' not found.");

        var pages = await pageRepository.FindAsync(
            p => p.TemplateKey == entity.Key, cancellationToken);

        if (pages.Count > 0)
            throw new InvalidOperationException(
                $"Template '{entity.Key}' is used by {pages.Count} page(s). Reassign them before deleting this template.");

        templateRepository.Delete(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
