using MediatR;
using Seems.Application.Pages.Dtos;

namespace Seems.Application.Pages.Commands.SetDefaultPage;

public record SetDefaultPageCommand(Guid Id) : IRequest<PageDto>;
