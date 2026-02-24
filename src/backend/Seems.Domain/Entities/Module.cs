using Seems.Domain.Common;
using Seems.Domain.Enums;

namespace Seems.Domain.Entities;

public class Module : BaseEntity, IAuditable
{
    public string ModuleKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public ModuleStatus Status { get; set; } = ModuleStatus.Installed;
    public string? PublicComponentUrl { get; set; }
}
