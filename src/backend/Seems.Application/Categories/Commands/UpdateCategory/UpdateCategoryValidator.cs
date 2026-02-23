using FluentValidation;

namespace Seems.Application.Categories.Commands.UpdateCategory;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Slug).MaximumLength(256).When(x => x.Slug is not null);
        RuleFor(x => x.Description).MaximumLength(1024).When(x => x.Description is not null);
    }
}
