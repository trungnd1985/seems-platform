using FluentValidation;

namespace Seems.Application.Content.Commands.CreateContentItem;

public class CreateContentItemValidator : AbstractValidator<CreateContentItemCommand>
{
    public CreateContentItemValidator()
    {
        RuleFor(x => x.ContentTypeKey).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Data).NotEmpty();
    }
}
