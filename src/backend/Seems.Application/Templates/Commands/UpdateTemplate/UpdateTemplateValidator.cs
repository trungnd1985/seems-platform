using FluentValidation;

namespace Seems.Application.Templates.Commands.UpdateTemplate;

public class UpdateTemplateValidator : AbstractValidator<UpdateTemplateCommand>
{
    public UpdateTemplateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);

        RuleFor(x => x.ThemeKey)
            .NotEmpty()
            .MaximumLength(128)
            .Matches(@"^[a-z][a-z0-9_-]*$")
            .WithMessage("ThemeKey must be lowercase, start with a letter, and contain only letters, digits, hyphens, or underscores.");

        RuleFor(x => x.Slots).NotNull();

        RuleForEach(x => x.Slots).ChildRules(slot =>
        {
            slot.RuleFor(s => s.Key)
                .NotEmpty()
                .MaximumLength(64)
                .Matches(@"^[a-z][a-z0-9_-]*$")
                .WithMessage("Slot key must be lowercase, start with a letter, and contain only letters, digits, hyphens, or underscores.");

            slot.RuleFor(s => s.Label).NotEmpty().MaximumLength(128);
            slot.RuleFor(s => s.MaxItems).GreaterThan(0).When(s => s.MaxItems.HasValue);
        });

        RuleFor(x => x.Slots)
            .Must(slots => slots.Select(s => s.Key).Distinct().Count() == slots.Count)
            .WithMessage("Slot keys must be unique within the template.");
    }
}
