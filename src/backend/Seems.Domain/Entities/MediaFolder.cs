using Seems.Domain.Common;

namespace Seems.Domain.Entities;

public class MediaFolder : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public Guid? ParentId { get; set; }

    public MediaFolder? Parent { get; set; }
    public ICollection<MediaFolder> Children { get; set; } = new List<MediaFolder>();
    public ICollection<Media> Items { get; set; } = new List<Media>();
}
