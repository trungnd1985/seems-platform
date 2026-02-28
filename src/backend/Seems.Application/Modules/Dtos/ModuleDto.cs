namespace Seems.Application.Modules.Dtos;

public class ModuleDto
{
    public Guid Id { get; set; }
    public string ModuleKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? PublicComponentUrl { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Minimal projection returned by the public /api/modules/installed endpoint.
/// Only exposes what the Nuxt plugin needs — no internal IDs.
/// </summary>
public class InstalledModuleDto
{
    public string ModuleKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? PublicComponentUrl { get; set; }
}
