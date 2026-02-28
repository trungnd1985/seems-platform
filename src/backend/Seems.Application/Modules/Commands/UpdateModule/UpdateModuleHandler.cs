using AutoMapper;
using MediatR;
using Seems.Application.Modules.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Modules.Commands.UpdateModule;

public class UpdateModuleHandler(
    IRepository<Module> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdateModuleCommand, ModuleDto>
{
    public async Task<ModuleDto> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Module '{request.Id}' not found.");

        module.Name = request.Name;
        module.Version = request.Version;
        module.PublicComponentUrl = request.PublicComponentUrl;
        module.Description = request.Description;
        module.Author = request.Author;

        repository.Update(module);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<ModuleDto>(module);
    }
}
