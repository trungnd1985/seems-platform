using AutoMapper;
using MediatR;
using Seems.Application.Pages.Dtos;
using Seems.Domain.Interfaces;

namespace Seems.Application.Pages.Commands.SetDefaultPage;

public class SetDefaultPageHandler(IPageRepository pageRepository, IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<SetDefaultPageCommand, PageDto>
{
    public async Task<PageDto> Handle(SetDefaultPageCommand request, CancellationToken cancellationToken)
    {
        var page = await pageRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Page '{request.Id}' not found.");

        if (page.IsDefault)
            return mapper.Map<PageDto>(page);

        // Unset any existing default
        var currentDefault = await pageRepository.GetDefaultAsync(cancellationToken);
        if (currentDefault is not null)
        {
            currentDefault.IsDefault = false;
            currentDefault.UpdatedAt = DateTime.UtcNow;
            pageRepository.Update(currentDefault);
        }

        page.IsDefault = true;
        page.UpdatedAt = DateTime.UtcNow;
        pageRepository.Update(page);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<PageDto>(page);
    }
}
