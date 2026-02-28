using MediatR;
using Seems.Domain.Entities;
using Seems.Domain.Enums;
using Seems.Domain.Interfaces;

namespace Seems.Application.Modules.Commands.DeleteModule;

public class DeleteModuleHandler(
    IRepository<Module> moduleRepository,
    IRepository<SlotMapping> slotRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteModuleCommand>
{
    public async Task Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await moduleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Module '{request.Id}' not found.");

        var orphanedSlots = await slotRepository.FindAsync(
            s => s.TargetType == SlotTargetType.Module && s.TargetId == module.ModuleKey,
            cancellationToken);

        foreach (var slot in orphanedSlots)
            slotRepository.Delete(slot);

        moduleRepository.Delete(module);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
