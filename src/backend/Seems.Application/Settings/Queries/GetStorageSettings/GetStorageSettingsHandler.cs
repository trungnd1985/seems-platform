using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Seems.Application.Common.Interfaces;
using Seems.Application.Settings.Dtos;

namespace Seems.Application.Settings.Queries.GetStorageSettings;

public class GetStorageSettingsHandler(IAppDbContext db)
    : IRequestHandler<GetStorageSettingsQuery, StorageSettingsDto>
{
    public async Task<StorageSettingsDto> Handle(GetStorageSettingsQuery request, CancellationToken cancellationToken)
    {
        var setting = await db.SiteSettings
            .FirstOrDefaultAsync(s => s.Key == "storage", cancellationToken);

        if (setting is null)
            return new StorageSettingsDto();

        try
        {
            return JsonSerializer.Deserialize<StorageSettingsDto>(setting.Value,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new StorageSettingsDto();
        }
        catch (JsonException)
        {
            return new StorageSettingsDto();
        }
    }
}
