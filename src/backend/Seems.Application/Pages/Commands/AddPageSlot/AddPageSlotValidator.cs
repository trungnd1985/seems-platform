using FluentValidation;

namespace Seems.Application.Pages.Commands.AddPageSlot;

public class AddPageSlotValidator : AbstractValidator<AddPageSlotCommand>
{
    public AddPageSlotValidator()
    {
        RuleFor(x => x.PageId).NotEmpty();
        RuleFor(x => x.SlotKey)
            .NotEmpty()
            .MaximumLength(64)
            .Matches(@"^[a-z][a-z0-9_-]*$")
            .WithMessage("SlotKey must be lowercase, start with a letter, and contain only letters, digits, hyphens, or underscores.");
        RuleFor(x => x.TargetId).NotEmpty().MaximumLength(256);
    }
}
