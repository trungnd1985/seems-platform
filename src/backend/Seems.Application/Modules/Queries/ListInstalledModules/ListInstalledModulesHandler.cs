using AutoMapper;
using MediatR;
using Seems.Application.Modules.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Enums;
using Seems.Domain.Interfaces;

namespace Seems.Application.Modules.Queries.ListInstalledModules;

public class ListInstalledModulesHandler(IRepository<Module> repository, IMapper mapper)
    : IRequestHandler<ListInstalledModulesQuery, IReadOnlyList<InstalledModuleDto>>
{
    public async Task<IReadOnlyList<InstalledModuleDto>> Handle(
        ListInstalledModulesQuery request,
        CancellationToken cancellationToken)
    {
        var modules = await repository.FindAsync(
            m => m.Status == ModuleStatus.Installed && m.PublicComponentUrl != null,
            cancellationToken);

        return mapper.Map<IReadOnlyList<InstalledModuleDto>>(modules);
    }
}
