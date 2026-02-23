using Seems.Domain.Common;

namespace Seems.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = [];
    public string ContentTypeKey { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public ICollection<ContentItemCategory> ContentItemCategories { get; set; } = [];
}
