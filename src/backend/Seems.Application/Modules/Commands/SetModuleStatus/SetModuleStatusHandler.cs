using AutoMapper;
using MediatR;
using Seems.Application.Modules.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Enums;
using Seems.Domain.Interfaces;

namespace Seems.Application.Modules.Commands.SetModuleStatus;

public class SetModuleStatusHandler(
    IRepository<Module> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<SetModuleStatusCommand, ModuleDto>
{
    public async Task<ModuleDto> Handle(SetModuleStatusCommand request, CancellationToken cancellationToken)
    {
        var module = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Module '{request.Id}' not found.");

        if (!Enum.TryParse<ModuleStatus>(request.Status, ignoreCase: true, out var status))
            throw new InvalidOperationException(
                $"Invalid module status '{request.Status}'. Valid values: Installed, Disabled.");

        module.Status = status;
        repository.Update(module);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<ModuleDto>(module);
    }
}
