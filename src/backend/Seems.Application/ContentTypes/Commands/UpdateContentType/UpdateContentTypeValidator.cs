using FluentValidation;

namespace Seems.Application.ContentTypes.Commands.UpdateContentType;

public class UpdateContentTypeValidator : AbstractValidator<UpdateContentTypeCommand>
{
    public UpdateContentTypeValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Schema).NotEmpty();
    }
}
