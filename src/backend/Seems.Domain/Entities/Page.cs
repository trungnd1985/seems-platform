using Seems.Domain.Common;
using Seems.Domain.Enums;
using Seems.Domain.ValueObjects;

namespace Seems.Domain.Entities;

public class Page : BaseEntity
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TemplateKey { get; set; } = string.Empty;
    public string? ThemeKey { get; set; }
    public SeoMeta Seo { get; set; } = new();
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public bool IsDefault { get; set; }

    public ICollection<SlotMapping> Slots { get; set; } = [];
}
