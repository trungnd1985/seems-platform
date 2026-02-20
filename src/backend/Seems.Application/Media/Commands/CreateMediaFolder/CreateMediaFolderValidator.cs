using FluentValidation;

namespace Seems.Application.Media.Commands.CreateMediaFolder;

public class CreateMediaFolderValidator : AbstractValidator<CreateMediaFolderCommand>
{
    public CreateMediaFolderValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
    }
}
