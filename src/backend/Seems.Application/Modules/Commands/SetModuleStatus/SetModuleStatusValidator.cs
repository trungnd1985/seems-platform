using FluentValidation;

namespace Seems.Application.Modules.Commands.SetModuleStatus;

public class SetModuleStatusValidator : AbstractValidator<SetModuleStatusCommand>
{
    private static readonly string[] ValidStatuses = ["Installed", "Disabled"];

    public SetModuleStatusValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => ValidStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Status must be 'Installed' or 'Disabled'.");
    }
}
