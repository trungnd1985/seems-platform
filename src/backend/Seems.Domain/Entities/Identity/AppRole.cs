using Microsoft.AspNetCore.Identity;

namespace Seems.Domain.Entities.Identity;

public class AppRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
