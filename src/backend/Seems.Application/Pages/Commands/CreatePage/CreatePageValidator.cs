using FluentValidation;

namespace Seems.Application.Pages.Commands.CreatePage;

public class CreatePageValidator : AbstractValidator<CreatePageCommand>
{
    public CreatePageValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(256)
            .Matches(@"^[a-z0-9][a-z0-9_-]*(?:\/[a-z0-9][a-z0-9_-]*)*$")
            .WithMessage("Slug must be lowercase alphanumeric segments separated by a single forward slash (e.g. about-us or blog/my-post). No leading/trailing slashes, consecutive slashes, or special characters.");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(512);
        RuleFor(x => x.TemplateKey).NotEmpty().MaximumLength(128);
    }
}
