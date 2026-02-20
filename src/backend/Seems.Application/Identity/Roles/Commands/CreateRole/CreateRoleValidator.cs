using FluentValidation;

namespace Seems.Application.Identity.Roles.Commands.CreateRole;

public class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z][a-zA-Z0-9\-_]*$")
            .WithMessage("Role name must start with a letter and contain only letters, digits, hyphens, or underscores.");

        RuleFor(x => x.Description)
            .MaximumLength(256)
            .When(x => x.Description is not null);
    }
}
