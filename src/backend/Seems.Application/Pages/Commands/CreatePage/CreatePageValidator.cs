using FluentValidation;

namespace Seems.Application.Pages.Commands.CreatePage;

public class CreatePageValidator : AbstractValidator<CreatePageCommand>
{
    public CreatePageValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(512);
        RuleFor(x => x.TemplateKey).NotEmpty().MaximumLength(128);
    }
}
