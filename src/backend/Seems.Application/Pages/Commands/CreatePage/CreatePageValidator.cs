using FluentValidation;

namespace Seems.Application.Pages.Commands.CreatePage;

public class CreatePageValidator : AbstractValidator<CreatePageCommand>
{
    public CreatePageValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(256)
            .Matches(@"^(:[a-z][a-z0-9-]*(\/:[a-z][a-z0-9-]*)*|[a-z0-9][a-z0-9_-]*)$")
            .WithMessage("Slug must be a lowercase alphanumeric segment (e.g. about-us) or one or more :param segments (e.g. :id or :year/:month/:slug).");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(512);
        RuleFor(x => x.TemplateKey).NotEmpty().MaximumLength(128);
    }
}
