namespace Seems.Domain.ValueObjects;

public class SeoMeta
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? OgTitle { get; set; }
    public string? OgDescription { get; set; }
    public string? OgImage { get; set; }
    public string? Canonical { get; set; }
    public string? Robots { get; set; }
}
