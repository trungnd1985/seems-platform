using MediatR;

namespace Seems.Application.Pages.Commands.DeletePage;

public record DeletePageCommand(Guid Id) : IRequest;
