using Seems.Domain.Common;

namespace Seems.Domain.Entities;

public class Theme : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
