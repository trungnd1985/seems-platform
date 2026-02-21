using MediatR;

namespace Seems.Application.Templates.Commands.DeleteTemplate;

public record DeleteTemplateCommand(Guid Id) : IRequest;
