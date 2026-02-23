using AutoMapper;
using MediatR;
using Seems.Application.Content.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Content.Commands.CreateContentItem;

public class CreateContentItemHandler(
    IContentRepository contentRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateContentItemCommand, ContentItemDto>
{
    public async Task<ContentItemDto> Handle(CreateContentItemCommand request, CancellationToken cancellationToken)
    {
        var item = new ContentItem
        {
            Id = Guid.NewGuid(),
            ContentTypeKey = request.ContentTypeKey,
            Data = request.Data,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await contentRepository.AddAsync(item, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.CategoryIds is not null)
        {
            await categoryRepository.SyncContentItemCategoriesAsync(item.Id, request.CategoryIds, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var saved = await contentRepository.GetByIdAsync(item.Id, cancellationToken) ?? item;
        return mapper.Map<ContentItemDto>(saved);
    }
}
