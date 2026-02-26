using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Seems.Application.Common.Interfaces;
using Seems.Domain.Entities;

namespace Seems.Application.Settings.Commands.UpdateSiteInfo;

public class UpdateSiteInfoHandler(IAppDbContext db)
    : IRequestHandler<UpdateSiteInfoCommand>
{
    private record StoredSiteInfo(
        string SiteName,
        string Tagline,
        string? LogoMediaId,
        string? FaviconMediaId);

    public async Task Handle(UpdateSiteInfoCommand request, CancellationToken cancellationToken)
    {
        var s = request.Settings;
        var stored = new StoredSiteInfo(s.SiteName, s.Tagline, s.LogoMediaId, s.FaviconMediaId);

        var json = JsonSerializer.Serialize(stored,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var setting = await db.SiteSettings
            .FirstOrDefaultAsync(x => x.Key == "site", cancellationToken);

        if (setting is null)
        {
            setting = new SiteSetting
            {
                Key = "site",
                Group = "site",
                Value = json,
                UpdatedAt = DateTime.UtcNow,
            };
            db.SiteSettings.Add(setting);
        }
        else
        {
            setting.Value = json;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
