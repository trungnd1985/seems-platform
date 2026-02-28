using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Seems.Application.Modules;

/// <summary>
/// Contract for first-party logic modules bundled with the platform.
/// Implement this interface once per <c>Seems.Modules.{Key}</c> class library.
///
/// At startup the platform scans every <c>Seems.Modules.*.dll</c> found in the
/// output directory, locates the <see cref="ISeemModule"/> implementation, and
/// automatically registers:
/// <list type="bullet">
///   <item>MVC application part (controllers)</item>
///   <item>MediatR handlers</item>
///   <item>FluentValidation validators</item>
///   <item>AutoMapper profiles</item>
/// </list>
/// Override <see cref="ConfigureServices"/> only when the module needs extra DI
/// registrations beyond the above defaults.
/// </summary>
public interface ISeemModule
{
    /// <summary>Must match <c>moduleKey</c> in the module's manifest.json.</summary>
    string ModuleKey { get; }

    /// <summary>
    /// Override to register module-specific services.
    /// The default implementation is a no-op.
    /// </summary>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration) { }
}
