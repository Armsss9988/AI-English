namespace EnglishCoach.Contracts.Shared;

public record ErrorResponse(string Code, string Message, string? Details = null);

public record PaginatedResponse<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage
);
