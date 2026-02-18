using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Seems.Domain.Entities;
using Seems.Domain.Entities.Identity;
using Seems.Domain.Enums;
using Seems.Domain.ValueObjects;

namespace Seems.Infrastructure.Persistence.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager);
            await SeedThemesAsync(context);
            await SeedTemplatesAsync(context);
            await SeedContentTypesAsync(context);
            await SeedSampleContentAsync(context);
            await SeedHomepageAsync(context);

            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
        }
    }

    private static async Task SeedRolesAsync(RoleManager<AppRole> roleManager)
    {
        string[] roles = ["Admin", "Editor", "Viewer"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new AppRole { Name = role });
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<AppUser> userManager)
    {
        const string adminEmail = "admin@seems.local";

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            DisplayName = "Admin",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(admin, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }

    private static async Task SeedThemesAsync(AppDbContext context)
    {
        if (await context.Themes.AnyAsync())
            return;

        context.Themes.Add(new Theme
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Key = "default",
            Name = "Default Theme",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedTemplatesAsync(AppDbContext context)
    {
        if (await context.Templates.AnyAsync())
            return;

        context.Templates.AddRange(
            new Template
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000001"),
                Key = "standard-page",
                Name = "Standard Page",
                ThemeKey = "default",
                Slots = """[{"key":"hero","label":"Hero"},{"key":"main","label":"Main Content"},{"key":"sidebar","label":"Sidebar"},{"key":"footer","label":"Footer"}]""",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new Template
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000002"),
                Key = "landing-page",
                Name = "Landing Page",
                ThemeKey = "default",
                Slots = """[{"key":"hero","label":"Hero"},{"key":"main","label":"Main Content"},{"key":"footer","label":"Footer"}]""",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new Template
            {
                Id = Guid.Parse("00000000-0000-0000-0001-000000000003"),
                Key = "blog-post",
                Name = "Blog Post",
                ThemeKey = "default",
                Slots = """[{"key":"main","label":"Main Content"},{"key":"footer","label":"Footer"}]""",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedContentTypesAsync(AppDbContext context)
    {
        if (await context.ContentTypes.AnyAsync())
            return;

        context.ContentTypes.AddRange(
            new ContentType
            {
                Id = Guid.Parse("00000000-0000-0000-0002-000000000001"),
                Key = "rich-text",
                Name = "Rich Text",
                Schema = """{"type":"object","properties":{"html":{"type":"string"}},"required":["html"]}""",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new ContentType
            {
                Id = Guid.Parse("00000000-0000-0000-0002-000000000002"),
                Key = "hero-block",
                Name = "Hero Block",
                Schema = """{"type":"object","properties":{"title":{"type":"string"},"subtitle":{"type":"string"},"backgroundImage":{"type":"string"},"ctaText":{"type":"string"},"ctaLink":{"type":"string"}},"required":["title"]}""",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new ContentType
            {
                Id = Guid.Parse("00000000-0000-0000-0002-000000000003"),
                Key = "image-block",
                Name = "Image Block",
                Schema = """{"type":"object","properties":{"src":{"type":"string"},"alt":{"type":"string"},"caption":{"type":"string"}},"required":["src","alt"]}""",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedSampleContentAsync(AppDbContext context)
    {
        if (await context.ContentItems.AnyAsync())
            return;

        context.ContentItems.AddRange(
            new ContentItem
            {
                Id = Guid.Parse("00000000-0000-0000-0003-000000000001"),
                ContentTypeKey = "hero-block",
                Data = """{"title":"Welcome to SEEMS","subtitle":"A modern headless CMS platform","ctaText":"Get Started","ctaLink":"/about"}""",
                Status = ContentStatus.Published,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new ContentItem
            {
                Id = Guid.Parse("00000000-0000-0000-0003-000000000002"),
                ContentTypeKey = "rich-text",
                Data = """{"html":"<h2>About SEEMS Platform</h2><p>SEEMS is a modular headless CMS built with ASP.NET Core and Nuxt 3. It combines WordPress flexibility with Strapi's headless approach.</p><p>Features include dynamic content types, template-based page composition, and a powerful module system.</p>"}""",
                Status = ContentStatus.Published,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task SeedHomepageAsync(AppDbContext context)
    {
        if (await context.Pages.AnyAsync())
            return;

        var homepage = new Page
        {
            Id = Guid.Parse("00000000-0000-0000-0004-000000000001"),
            Slug = "/",
            Title = "Home",
            TemplateKey = "standard-page",
            ThemeKey = "default",
            Seo = new SeoMeta
            {
                Title = "SEEMS Platform — Modern Headless CMS",
                Description = "A modular headless CMS built with ASP.NET Core and Nuxt 3.",
                OgTitle = "SEEMS Platform",
                OgDescription = "WordPress flexibility + Strapi headless + ASP.NET Core + Nuxt 3",
            },
            Status = ContentStatus.Published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        homepage.Slots.Add(new SlotMapping
        {
            Id = Guid.NewGuid(),
            SlotKey = "hero",
            TargetType = SlotTargetType.Content,
            TargetId = "00000000-0000-0000-0003-000000000001",
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });

        homepage.Slots.Add(new SlotMapping
        {
            Id = Guid.NewGuid(),
            SlotKey = "main",
            TargetType = SlotTargetType.Content,
            TargetId = "00000000-0000-0000-0003-000000000002",
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });

        context.Pages.Add(homepage);
        await context.SaveChangesAsync();
    }
}
