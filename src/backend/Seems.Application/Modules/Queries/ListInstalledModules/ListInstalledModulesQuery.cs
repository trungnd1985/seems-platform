using MediatR;
using Seems.Application.Modules.Dtos;

namespace Seems.Application.Modules.Queries.ListInstalledModules;

public record ListInstalledModulesQuery : IRequest<IReadOnlyList<InstalledModuleDto>>;
