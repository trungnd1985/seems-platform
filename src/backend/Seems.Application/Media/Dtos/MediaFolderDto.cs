namespace Seems.Application.Media.Dtos;

public class MediaFolderDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public Guid? ParentId { get; set; }
    public int ChildCount { get; set; }
    public int MediaCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
