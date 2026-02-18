using Seems.Domain.Common;

namespace Seems.Domain.Entities;

public class ContentType : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Schema { get; set; } = "{}";
}
