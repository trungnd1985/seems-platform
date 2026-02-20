using FluentValidation;

namespace Seems.Application.ContentTypes.Commands.CreateContentType;

public class CreateContentTypeValidator : AbstractValidator<CreateContentTypeCommand>
{
    public CreateContentTypeValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .MaximumLength(128)
            .Matches(@"^[a-z][a-z0-9_-]*$")
            .WithMessage("Key must be lowercase, start with a letter, and contain only letters, digits, hyphens, or underscores.");

        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Schema).NotEmpty();
    }
}
