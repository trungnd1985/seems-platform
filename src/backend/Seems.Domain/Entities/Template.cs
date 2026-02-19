using Seems.Domain.Common;

namespace Seems.Domain.Entities;

public class Template : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ThemeKey { get; set; } = string.Empty;
    public string Slots { get; set; } = "[]";
}
