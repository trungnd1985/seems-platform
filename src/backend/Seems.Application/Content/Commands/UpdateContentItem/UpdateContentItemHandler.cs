using AutoMapper;
using MediatR;
using Seems.Application.Content.Dtos;
using Seems.Domain.Enums;
using Seems.Domain.Interfaces;

namespace Seems.Application.Content.Commands.UpdateContentItem;

public class UpdateContentItemHandler(
    IContentRepository contentRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdateContentItemCommand, ContentItemDto>
{
    public async Task<ContentItemDto> Handle(UpdateContentItemCommand request, CancellationToken cancellationToken)
    {
        var item = await contentRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Content item '{request.Id}' not found.");

        item.Data = request.Data;
        item.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<ContentStatus>(request.Status, true, out var status))
        {
            item.Status = status;
        }

        contentRepository.Update(item);

        if (request.CategoryIds is not null)
        {
            await categoryRepository.SyncContentItemCategoriesAsync(item.Id, request.CategoryIds, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = await contentRepository.GetByIdAsync(item.Id, cancellationToken) ?? item;
        return mapper.Map<ContentItemDto>(saved);
    }
}
