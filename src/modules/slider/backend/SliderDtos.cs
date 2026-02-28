namespace Seems.Modules.Slider;

public class SlideDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? CtaText { get; set; }
    public string? CtaLink { get; set; }
    public int Order { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public record CreateSlideRequest(
    string Title,
    string ImageUrl,
    string? Subtitle = null,
    string? CtaText = null,
    string? CtaLink = null,
    int Order = 0
);

public record UpdateSlideRequest(
    string Title,
    string ImageUrl,
    string Status,
    string? Subtitle = null,
    string? CtaText = null,
    string? CtaLink = null,
    int Order = 0
);
