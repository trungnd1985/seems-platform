namespace Seems.Application.Content.Dtos;

public class ContentItemDto
{
    public Guid Id { get; set; }
    public string ContentTypeKey { get; set; } = string.Empty;
    public string Data { get; set; } = "{}";
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
