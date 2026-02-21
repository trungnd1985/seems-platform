using AutoMapper;
using MediatR;
using Seems.Application.Pages.Dtos;
using Seems.Domain.Interfaces;

namespace Seems.Application.Pages.Commands.UpdatePageStatus;

public class UpdatePageStatusHandler(IPageRepository pageRepository, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<UpdatePageStatusCommand, PageDto>
{
    public async Task<PageDto> Handle(UpdatePageStatusCommand request, CancellationToken cancellationToken)
    {
        var page = await pageRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Page '{request.Id}' not found.");

        page.Status = request.Status;
        page.UpdatedAt = DateTime.UtcNow;

        pageRepository.Update(page);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<PageDto>(page);
    }
}
