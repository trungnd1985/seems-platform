using FluentValidation;

namespace Seems.Application.Modules.Commands.RegisterModule;

public class RegisterModuleValidator : AbstractValidator<RegisterModuleCommand>
{
    public RegisterModuleValidator()
    {
        RuleFor(x => x.ModuleKey)
            .NotEmpty()
            .MaximumLength(128)
            .Matches(@"^[a-z][a-z0-9-]*$")
            .WithMessage("Module key must start with a lowercase letter and contain only lowercase letters, digits, or hyphens.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Version)
            .NotEmpty()
            .MaximumLength(64)
            .Matches(@"^\d+\.\d+(\.\d+)?")
            .WithMessage("Version must follow semantic versioning (e.g. 1.0.0).");

        RuleFor(x => x.PublicComponentUrl)
            .Must(url => url!.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("/"))
            .WithMessage("Public component URL must start with '/' (relative) or 'https://'.")
            .MaximumLength(2048)
            .When(x => x.PublicComponentUrl is not null);

        RuleFor(x => x.Description)
            .MaximumLength(512)
            .When(x => x.Description is not null);

        RuleFor(x => x.Author)
            .MaximumLength(256)
            .When(x => x.Author is not null);
    }
}
