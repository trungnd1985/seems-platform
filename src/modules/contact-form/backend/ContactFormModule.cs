using Seems.Application.Modules;

namespace Seems.Modules.ContactForm;

/// <summary>
/// Module entry point auto-discovered by the platform at startup.
/// MVC controllers, MediatR handlers, validators, and AutoMapper profiles
/// in this assembly are registered without any manual wiring.
/// </summary>
public class ContactFormModule : ISeemModule
{
    public string ModuleKey => "contact-form";
}
