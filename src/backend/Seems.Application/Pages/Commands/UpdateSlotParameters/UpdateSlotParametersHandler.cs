using System.Text.Json;
using MediatR;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Pages.Commands.UpdateSlotParameters;

public class UpdateSlotParametersHandler(
    IPageRepository pageRepository,
    IRepository<SlotMapping> slotRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSlotParametersCommand>
{
    public async Task Handle(UpdateSlotParametersCommand request, CancellationToken cancellationToken)
    {
        var page = await pageRepository.GetByIdAsync(request.PageId, cancellationToken)
            ?? throw new KeyNotFoundException($"Page '{request.PageId}' not found.");

        var mapping = await slotRepository.GetByIdAsync(request.SlotId, cancellationToken);
        if (mapping is null || mapping.PageId != request.PageId)
            throw new KeyNotFoundException($"Slot mapping '{request.SlotId}' not found on page '{request.PageId}'.");

        mapping.Parameters = request.Parameters is null or { Count: 0 }
            ? null
            : JsonSerializer.Serialize(request.Parameters);

        mapping.UpdatedAt = DateTime.UtcNow;
        slotRepository.Update(mapping);

        page.UpdatedAt = DateTime.UtcNow;
        pageRepository.Update(page);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
