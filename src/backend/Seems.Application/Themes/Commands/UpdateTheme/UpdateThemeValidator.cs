using FluentValidation;

namespace Seems.Application.Themes.Commands.UpdateTheme;

public class UpdateThemeValidator : AbstractValidator<UpdateThemeCommand>
{
    public UpdateThemeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(1024).When(x => x.Description is not null);
    }
}
