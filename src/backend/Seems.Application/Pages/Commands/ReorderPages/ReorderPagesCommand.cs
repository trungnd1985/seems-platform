using MediatR;

namespace Seems.Application.Pages.Commands.ReorderPages;

public record PageSortItem(Guid PageId, int SortOrder);

public record ReorderPagesCommand(IReadOnlyList<PageSortItem> Items) : IRequest;
