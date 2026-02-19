namespace Seems.Application.Common.Models;

public class PaginatedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Total { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);

    public PaginatedList(IReadOnlyList<T> items, int total, int page, int pageSize)
    {
        Items = items;
        Total = total;
        Page = page;
        PageSize = pageSize;
    }
}
