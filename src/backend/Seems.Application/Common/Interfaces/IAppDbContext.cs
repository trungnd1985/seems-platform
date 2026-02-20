using Microsoft.EntityFrameworkCore;
using Seems.Domain.Entities;
using Seems.Domain.Entities.Identity;

namespace Seems.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Domain.Entities.Media> Media { get; }
    DbSet<MediaFolder> MediaFolders { get; }
    DbSet<SiteSetting> SiteSettings { get; }
    DbSet<AppUser> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
