using MediatR;
using Seems.Domain.Interfaces;

namespace Seems.Application.Categories.Commands.DeleteCategory;

public class DeleteCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category '{request.Id}' not found.");

        var children = await categoryRepository.FindAsync(
            c => c.ParentId == request.Id, cancellationToken);

        if (children.Count > 0)
            throw new InvalidOperationException("Cannot delete a category that has sub-categories. Delete or reassign them first.");

        categoryRepository.Delete(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
