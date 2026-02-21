using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Seems.Application.Common.Interfaces;
using Seems.Domain.Entities;
using Seems.Domain.Entities.Identity;
using Seems.Domain.Interfaces;
using Seems.Infrastructure.Identity;
using Seems.Infrastructure.Persistence;
using Seems.Infrastructure.Persistence.Repositories;
using Seems.Infrastructure.Storage;

namespace Seems.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IPageRepository, PageRepository>();
        services.AddScoped<IContentRepository, ContentRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddHttpContextAccessor();
        services.AddScoped<IPreviewContext, PreviewContext>();

        // Storage providers
        services.AddScoped<IStorageProvider, LocalStorageProvider>();
        services.AddScoped<IStorageProvider, S3StorageProvider>();
        services.AddScoped<IStorageProviderFactory, StorageProviderFactory>();

        return services;
    }
}
