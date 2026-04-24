namespace EnglishCoach.SharedKernel.Pagination;

public record PageResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
)
{
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

public static class PageResult
{
    public static PageResult<T> Create<T>(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PageResult<T>(items, totalCount, page, pageSize, totalPages);
    }
}