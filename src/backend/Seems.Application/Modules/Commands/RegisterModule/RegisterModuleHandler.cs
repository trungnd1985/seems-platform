using AutoMapper;
using MediatR;
using Seems.Application.Modules.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Modules.Commands.RegisterModule;

public class RegisterModuleHandler(
    IRepository<Module> moduleRepository,
    IRepository<ContentType> contentTypeRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<RegisterModuleCommand, ModuleDto>
{
    public async Task<ModuleDto> Handle(RegisterModuleCommand request, CancellationToken cancellationToken)
    {
        var existing = await moduleRepository.FindAsync(
            m => m.ModuleKey == request.ModuleKey, cancellationToken);

        if (existing.Count > 0)
            throw new InvalidOperationException($"A module with key '{request.ModuleKey}' is already registered.");

        var module = new Module
        {
            Id = Guid.NewGuid(),
            ModuleKey = request.ModuleKey,
            Name = request.Name,
            Version = request.Version,
            PublicComponentUrl = request.PublicComponentUrl,
            Description = request.Description,
            Author = request.Author,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await moduleRepository.AddAsync(module, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.ContentTypes is { Count: > 0 })
        {
            foreach (var decl in request.ContentTypes)
            {
                var existingCt = await contentTypeRepository.FindAsync(
                    ct => ct.Key == decl.Key, cancellationToken);

                if (existingCt.Count > 0)
                    continue;

                var contentType = new ContentType
                {
                    Id = Guid.NewGuid(),
                    Key = decl.Key,
                    Name = decl.Name,
                    Schema = decl.Schema,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                await contentTypeRepository.AddAsync(contentType, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return mapper.Map<ModuleDto>(module);
    }
}
