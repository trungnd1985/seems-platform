using Seems.Domain.Common;

namespace Seems.Domain.Entities;

public class Media : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
}
