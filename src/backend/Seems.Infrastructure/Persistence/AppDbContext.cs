using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Seems.Application.Common.Interfaces;
using Seems.Domain.Entities;
using Seems.Domain.Entities.Identity;

namespace Seems.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, AppRole, Guid>(options), IAppDbContext
{
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<SlotMapping> SlotMappings => Set<SlotMapping>();
    public DbSet<ContentType> ContentTypes => Set<ContentType>();
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<Theme> Themes => Set<Theme>();
    public DbSet<Domain.Entities.Media> Media => Set<Domain.Entities.Media>();
    public DbSet<MediaFolder> MediaFolders => Set<MediaFolder>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ContentItemCategory> ContentItemCategories => Set<ContentItemCategory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
