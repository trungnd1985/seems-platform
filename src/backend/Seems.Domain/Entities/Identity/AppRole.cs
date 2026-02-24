using Microsoft.AspNetCore.Identity;
using Seems.Domain.Common;

namespace Seems.Domain.Entities.Identity;

public class AppRole : IdentityRole<Guid>, IAuditable
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
