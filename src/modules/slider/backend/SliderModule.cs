using Seems.Application.Modules;

namespace Seems.Modules.Slider;

/// <summary>
/// Module entry point discovered automatically by the platform at startup.
/// MVC controllers, MediatR handlers, validators, and AutoMapper profiles
/// in this assembly are registered without any manual wiring.
/// </summary>
public class SliderModule : ISeemModule
{
    public string ModuleKey => "slider";

    // No extra DI — everything is auto-registered by the platform scanner.
}
