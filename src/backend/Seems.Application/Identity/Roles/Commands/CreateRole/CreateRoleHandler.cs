using MediatR;
using Microsoft.AspNetCore.Identity;
using Seems.Application.Identity.Roles.Dtos;
using Seems.Domain.Entities.Identity;

namespace Seems.Application.Identity.Roles.Commands.CreateRole;

public class CreateRoleHandler(RoleManager<AppRole> roleManager)
    : IRequestHandler<CreateRoleCommand, RoleDto>
{
    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var existing = await roleManager.FindByNameAsync(request.Name);
        if (existing is not null)
            throw new InvalidOperationException($"Role '{request.Name}' already exists.");

        var role = new AppRole
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(result.Errors.First().Description);

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name!,
            Description = role.Description,
            UserCount = 0,
            IsSystem = false,
            CreatedAt = role.CreatedAt,
        };
    }
}
