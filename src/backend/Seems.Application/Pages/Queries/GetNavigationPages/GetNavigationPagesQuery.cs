using MediatR;

namespace Seems.Application.Pages.Queries.GetNavigationPages;

/// <summary>Returns the published navigation tree (pages with ShowInNavigation = true),
/// ordered by SortOrder, excluding the root ("/") page.</summary>
public record GetNavigationPagesQuery : IRequest<List<NavPageItem>>;

/// <summary>A single node in the public navigation tree.</summary>
public record NavPageItem(string Label, string Slug, string Path, List<NavPageItem> Children);
