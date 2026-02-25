using FluentValidation;

namespace Seems.Application.Pages.Commands.CreatePage;

public class CreatePageValidator : AbstractValidator<CreatePageCommand>
{
    public CreatePageValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(256)
            .Matches(@"^[a-z0-9][a-z0-9_-]*$")
            .WithMessage("Slug must be a single lowercase alphanumeric segment (e.g. about-us, careers). No slashes — use the Parent field to build nested paths.");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(512);
        RuleFor(x => x.TemplateKey).NotEmpty().MaximumLength(128);
    }
}
