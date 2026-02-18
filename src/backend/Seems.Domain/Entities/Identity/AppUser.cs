using Microsoft.AspNetCore.Identity;

namespace Seems.Domain.Entities.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
