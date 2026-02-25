using Seems.Domain.ValueObjects;

namespace Seems.Application.Pages.Dtos;

public class PageDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string? ParentTitle { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TemplateKey { get; set; } = string.Empty;
    public string? ThemeKey { get; set; }
    public SeoMeta Seo { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool ShowInNavigation { get; set; }
    public ICollection<SlotMappingDto> Slots { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SlotMappingDto
{
    public Guid Id { get; set; }
    public string SlotKey { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public int Order { get; set; }
}
