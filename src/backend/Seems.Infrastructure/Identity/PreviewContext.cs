using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Seems.Application.Common.Interfaces;

namespace Seems.Infrastructure.Identity;

public class PreviewContext(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    : IPreviewContext
{
    public bool IsPreview
    {
        get
        {
            var secret = configuration["Preview:Secret"];
            if (string.IsNullOrWhiteSpace(secret))
                return false;

            var token = httpContextAccessor.HttpContext?
                .Request.Headers["X-Preview-Token"]
                .FirstOrDefault();

            return !string.IsNullOrWhiteSpace(token)
                && string.Equals(token, secret, StringComparison.Ordinal);
        }
    }
}
