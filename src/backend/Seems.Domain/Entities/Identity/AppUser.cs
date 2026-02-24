using Microsoft.AspNetCore.Identity;
using Seems.Domain.Common;

namespace Seems.Domain.Entities.Identity;

public class AppUser : IdentityUser<Guid>, IAuditable
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
