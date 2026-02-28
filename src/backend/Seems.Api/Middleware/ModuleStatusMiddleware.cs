using Seems.Domain.Entities;
using Seems.Domain.Enums;
using Seems.Domain.Interfaces;

namespace Seems.Api.Middleware;

/// <summary>
/// Intercepts requests to /api/modules/{key}/{action} and returns 404
/// if the target module does not exist or is Disabled.
/// Admin management routes (/api/modules, /api/modules/{guid}) are not affected
/// because they lack a sub-action segment.
/// </summary>
public class ModuleStatusMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
    {
        var segments = context.Request.Path.Value?
            .TrimStart('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            ?? [];

        // Only intercept /api/modules/{key}/{action...} — 4+ segments
        if (segments.Length >= 4
            && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase)
            && segments[1].Equals("modules", StringComparison.OrdinalIgnoreCase))
        {
            var moduleKey = segments[2];

            using var scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<Module>>();

            var matches = await repo.FindAsync(
                m => m.ModuleKey == moduleKey,
                context.RequestAborted);

            var module = matches.FirstOrDefault();

            if (module is null || module.Status == ModuleStatus.Disabled)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    $"{{\"status\":404,\"message\":\"Module '{moduleKey}' not found or is disabled.\"}}",
                    context.RequestAborted);
                return;
            }
        }

        await next(context);
    }
}
