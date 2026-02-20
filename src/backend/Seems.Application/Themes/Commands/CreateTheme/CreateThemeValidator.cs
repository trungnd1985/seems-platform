using FluentValidation;

namespace Seems.Application.Themes.Commands.CreateTheme;

public class CreateThemeValidator : AbstractValidator<CreateThemeCommand>
{
    public CreateThemeValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .MaximumLength(128)
            .Matches(@"^[a-z][a-z0-9_-]*$")
            .WithMessage("Key must be lowercase, start with a letter, and contain only letters, digits, hyphens, or underscores.");

        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(1024).When(x => x.Description is not null);
    }
}
