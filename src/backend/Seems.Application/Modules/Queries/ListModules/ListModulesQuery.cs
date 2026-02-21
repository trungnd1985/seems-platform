using MediatR;
using Seems.Application.Modules.Dtos;

namespace Seems.Application.Modules.Queries.ListModules;

public record ListModulesQuery : IRequest<IReadOnlyList<ModuleDto>>;
