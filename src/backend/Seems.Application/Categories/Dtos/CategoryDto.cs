namespace Seems.Application.Categories.Dtos;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public string ContentTypeKey { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public int ItemCount { get; set; }
    public IReadOnlyList<CategoryDto> Children { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
