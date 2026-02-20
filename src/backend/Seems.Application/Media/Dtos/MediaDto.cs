namespace Seems.Application.Media.Dtos;

public class MediaDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? AltText { get; set; }
    public string? Caption { get; set; }
    public Guid OwnerId { get; set; }
    public string? OwnerEmail { get; set; }
    public Guid? FolderId { get; set; }
    public DateTime CreatedAt { get; set; }
}
