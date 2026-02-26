namespace Seems.Application.Settings.Dtos;

public class SiteInfoDto
{
    public string SiteName { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public string? LogoMediaId { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconMediaId { get; set; }
    public string? FaviconUrl { get; set; }
}
