using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Seems.Application.Common.Interfaces;
using Seems.Application.Settings.Dtos;

namespace Seems.Application.Settings.Queries.GetSiteInfo;

public class GetSiteInfoHandler(IAppDbContext db)
    : IRequestHandler<GetSiteInfoQuery, SiteInfoDto>
{
    public async Task<SiteInfoDto> Handle(GetSiteInfoQuery request, CancellationToken cancellationToken)
    {
        var setting = await db.SiteSettings
            .FirstOrDefaultAsync(s => s.Key == "site", cancellationToken);

        SiteInfoDto dto;
        if (setting is null)
        {
            dto = new SiteInfoDto();
        }
        else
        {
            try
            {
                dto = JsonSerializer.Deserialize<SiteInfoDto>(setting.Value,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new SiteInfoDto();
            }
            catch (JsonException)
            {
                dto = new SiteInfoDto();
            }
        }

        if (dto.LogoMediaId is not null && Guid.TryParse(dto.LogoMediaId, out var logoId))
        {
            var logo = await db.Media.FirstOrDefaultAsync(m => m.Id == logoId, cancellationToken);
            dto.LogoUrl = logo?.Url;
        }

        if (dto.FaviconMediaId is not null && Guid.TryParse(dto.FaviconMediaId, out var faviconId))
        {
            var favicon = await db.Media.FirstOrDefaultAsync(m => m.Id == faviconId, cancellationToken);
            dto.FaviconUrl = favicon?.Url;
        }

        return dto;
    }
}
