using FluentValidation;

namespace Seems.Application.Media.Commands.RenameMediaFolder;

public class RenameMediaFolderValidator : AbstractValidator<RenameMediaFolderCommand>
{
    public RenameMediaFolderValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
    }
}
