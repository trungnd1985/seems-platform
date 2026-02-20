namespace Seems.Domain.Entities;

public class SiteSetting
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
