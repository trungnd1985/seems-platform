using AutoMapper;
using MediatR;
using Seems.Application.Modules.Dtos;
using Seems.Domain.Entities;
using Seems.Domain.Interfaces;

namespace Seems.Application.Modules.Queries.ListModules;

public class ListModulesHandler(IRepository<Module> repository, IMapper mapper)
    : IRequestHandler<ListModulesQuery, IReadOnlyList<ModuleDto>>
{
    public async Task<IReadOnlyList<ModuleDto>> Handle(ListModulesQuery request, CancellationToken cancellationToken)
    {
        var modules = await repository.GetAllAsync(cancellationToken);
        return mapper.Map<IReadOnlyList<ModuleDto>>(modules);
    }
}
