using AutoMapper;
using MediatR;
using Seems.Application.Pages.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Pages.Commands.AddPageSlot;

public class AddPageSlotHandler(
    IPageRepository pageRepository,
    IRepository<SlotMapping> slotRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<AddPageSlotCommand, SlotMappingDto>
{
    public async Task<SlotMappingDto> Handle(AddPageSlotCommand request, CancellationToken cancellationToken)
    {
        var page = await pageRepository.GetWithSlotsAsync(request.PageId, cancellationToken)
            ?? throw new KeyNotFoundException($"Page '{request.PageId}' not found.");

        // Auto-assign order: max existing order within this slot key + 1
        var nextOrder = page.Slots
            .Where(s => s.SlotKey == request.SlotKey)
            .Select(s => s.Order)
            .DefaultIfEmpty(-1)
            .Max() + 1;

        var mapping = new SlotMapping
        {
            Id = Guid.NewGuid(),
            PageId = request.PageId,
            SlotKey = request.SlotKey,
            TargetType = request.TargetType,
            TargetId = request.TargetId,
            Order = nextOrder,
        };

        await slotRepository.AddAsync(mapping, cancellationToken);

        page.UpdatedAt = DateTime.UtcNow;
        pageRepository.Update(page);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<SlotMappingDto>(mapping);
    }
}
