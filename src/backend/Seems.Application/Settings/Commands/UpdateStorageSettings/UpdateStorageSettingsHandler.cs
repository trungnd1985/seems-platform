using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Seems.Application.Common.Interfaces;
using Seems.Domain.Entities;

namespace Seems.Application.Settings.Commands.UpdateStorageSettings;

public class UpdateStorageSettingsHandler(IAppDbContext db)
    : IRequestHandler<UpdateStorageSettingsCommand>
{
    public async Task Handle(UpdateStorageSettingsCommand request, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(request.Settings,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var setting = await db.SiteSettings
            .FirstOrDefaultAsync(s => s.Key == "storage", cancellationToken);

        if (setting is null)
        {
            setting = new SiteSetting
            {
                Key = "storage",
                Group = "storage",
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
