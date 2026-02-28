using MediatR;
using Seems.Application.Modules.Dtos;

namespace Seems.Application.Modules.Commands.RegisterModule;

public record ContentTypeDecl(string Key, string Name, string Schema);

public record RegisterModuleCommand(
    string ModuleKey,
    string Name,
    string Version,
    string? PublicComponentUrl,
    string? Description,
    string? Author,
    IReadOnlyList<ContentTypeDecl>? ContentTypes
) : IRequest<ModuleDto>;
