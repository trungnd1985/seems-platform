namespace Seems.Application.Content.Dtos;

public class ContentTypeDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Schema { get; set; } = "{}";
}
