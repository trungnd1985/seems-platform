using Seems.Domain.Common;
using Seems.Domain.Enums;

namespace Seems.Domain.Entities;

public class ContentItem : BaseEntity
{
    public string ContentTypeKey { get; set; } = string.Empty;
    public string Data { get; set; } = "{}";
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
}
