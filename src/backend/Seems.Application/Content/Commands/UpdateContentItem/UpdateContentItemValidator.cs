using FluentValidation;
using Seems.Domain.Enums;

namespace Seems.Application.Content.Commands.UpdateContentItem;

public class UpdateContentItemValidator : AbstractValidator<UpdateContentItemCommand>
{
    private static readonly string[] ValidStatuses = Enum.GetNames<ContentStatus>();

    public UpdateContentItemValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Data).NotEmpty();
        RuleFor(x => x.Status)
            .Must(s => s is null || ValidStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}");
    }
}
