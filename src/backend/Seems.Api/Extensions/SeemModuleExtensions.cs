using System.Reflection;
using FluentValidation;
using Seems.Application.Modules;

namespace Seems.Api.Extensions;

/// <summary>
/// Scans the output directory for <c>Seems.Modules.*.dll</c> assemblies and
/// auto-registers their controllers, MediatR handlers, validators, and AutoMapper
/// profiles. Module developers only need to implement <see cref="ISeemModule"/>;
/// no manual wiring in Program.cs is required.
/// </summary>
public static class SeemModuleExtensions
{
    private const string Prefix = "Seems.Modules.";

    public static IMvcBuilder LoadSeemModules(this IMvcBuilder mvcBuilder, IConfiguration configuration)
    {
        var services = mvcBuilder.Services;
        var baseDir = AppContext.BaseDirectory;

        foreach (var path in Directory.EnumerateFiles(baseDir, $"{Prefix}*.dll"))
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFrom(path);
            }
            catch (Exception ex)
            {
                // Log and skip assemblies that fail to load (corrupt, wrong runtime, etc.)
                Console.Error.WriteLine($"[SeemModules] Failed to load {path}: {ex.Message}");
                continue;
            }

            var moduleType = assembly.GetTypes().FirstOrDefault(
                t => typeof(ISeemModule).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

            if (moduleType is null)
            {
                Console.Error.WriteLine($"[SeemModules] No ISeemModule found in {Path.GetFileName(path)} — skipped.");
                continue;
            }

            var module = (ISeemModule)Activator.CreateInstance(moduleType)!;

            // Module-specific services (opt-in)
            module.ConfigureServices(services, configuration);

            // Auto-register MediatR handlers from this assembly.
            // Pipeline behaviors (ValidationBehavior, LoggingBehavior) are already registered
            // globally by AddApplication() and apply to all handlers in the container.
            services.AddMediatR(cfg =>
            {
                cfg.LicenseKey = configuration["MediatR:LicenseKey"];
                cfg.RegisterServicesFromAssembly(assembly);
            });

            // Auto-register validators and AutoMapper profiles
            services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
            services.AddAutoMapper(assembly);

            // Make the module's controllers discoverable by MVC
            mvcBuilder.AddApplicationPart(assembly);

            Console.WriteLine($"[SeemModules] Loaded module '{module.ModuleKey}' from {Path.GetFileName(path)}");
        }

        return mvcBuilder;
    }
}
