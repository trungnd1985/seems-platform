using AutoMapper;
using MediatR;
using Seems.Application.Content.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Content.Commands.CreateContentItem;

public class CreateContentItemHandler(IContentRepository contentRepository, IUnitOfWork unitOfWork, IMapper mapper)
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

        return mapper.Map<ContentItemDto>(item);
    }
}
