using FluentValidation;

namespace Seems.Application.Categories.Commands.CreateCategory;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Slug).MaximumLength(256).When(x => x.Slug is not null);
        RuleFor(x => x.Description).MaximumLength(1024).When(x => x.Description is not null);
        RuleFor(x => x.ContentTypeKey).NotEmpty().MaximumLength(128);
    }
}
