using Seems.Domain.Entities.Identity;

namespace Seems.Application.Common.Interfaces;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(AppUser user);
}
