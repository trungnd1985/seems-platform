using MediatR;
using Seems.Domain.Interfaces;

namespace Seems.Application.Pages.Queries.GetNavigationPages;

public class GetNavigationPagesHandler(IPageRepository pageRepository)
    : IRequestHandler<GetNavigationPagesQuery, List<NavPageItem>>
{
    public async Task<List<NavPageItem>> Handle(GetNavigationPagesQuery request, CancellationToken cancellationToken)
    {
        var pages = await pageRepository.GetPublishedPagesAsync(cancellationToken);

        var lookup = pages
            .Where(p => p.ShowInNavigation && p.Slug != "/")
            .ToLookup(p => p.ParentId);

        List<NavPageItem> BuildChildren(Guid? parentId) =>
            lookup[parentId]
                .Select(p => new NavPageItem(p.Title, p.Slug, p.Path, BuildChildren(p.Id)))
                .ToList();

        return BuildChildren(null);
    }
}
