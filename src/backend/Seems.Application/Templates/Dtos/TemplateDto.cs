namespace Seems.Application.Templates.Dtos;

public class TemplateDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ThemeKey { get; set; } = string.Empty;
    public bool ThemeExists { get; set; }
    public IReadOnlyList<TemplateSlotDef> Slots { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
