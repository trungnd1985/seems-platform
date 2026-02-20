using FluentValidation;

namespace Seems.Application.Media.Commands.UploadMedia;

public class UploadMediaValidator : AbstractValidator<UploadMediaCommand>
{
    public UploadMediaValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(512);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Size).GreaterThan(0).WithMessage("File must not be empty.");
        RuleFor(x => x.Content).NotEmpty().WithMessage("File content is required.");
    }
}
