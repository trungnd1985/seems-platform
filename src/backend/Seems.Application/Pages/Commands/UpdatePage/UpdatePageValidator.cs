using FluentValidation;

namespace Seems.Application.Pages.Commands.UpdatePage;

public class UpdatePageValidator : AbstractValidator<UpdatePageCommand>
{
    public UpdatePageValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(256)
            .Matches(@"^[a-z0-9][a-z0-9/_-]*$")
            .WithMessage("Slug must be lowercase and contain only letters, digits, hyphens, underscores, or forward slashes.");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(512);
        RuleFor(x => x.TemplateKey).NotEmpty().MaximumLength(128);
        RuleFor(x => x.ThemeKey).MaximumLength(128).When(x => x.ThemeKey is not null);
    }
}
