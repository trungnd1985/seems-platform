using FluentValidation;

namespace Seems.Application.Modules.Commands.UpdateModule;

public class UpdateModuleValidator : AbstractValidator<UpdateModuleCommand>
{
    public UpdateModuleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Version)
            .NotEmpty()
            .MaximumLength(64)
            .Matches(@"^\d+\.\d+(\.\d+)?")
            .WithMessage("Version must follow semantic versioning (e.g. 1.0.0).");

        RuleFor(x => x.PublicComponentUrl)
            .Must(BeAValidComponentUrl)
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

    private static bool BeAValidComponentUrl(string? url) =>
        url is not null &&
        (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
         url.StartsWith("/"));
}
