using Seems.Domain.Common;

namespace Seems.Domain.Entities;

public class Media : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? AltText { get; set; }
    public string? Caption { get; set; }

    public Guid OwnerId { get; set; }
    public Guid? FolderId { get; set; }

    public MediaFolder? Folder { get; set; }
}
