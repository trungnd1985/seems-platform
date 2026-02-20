namespace Seems.Application.Identity.Roles.Dtos;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UserCount { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
}
