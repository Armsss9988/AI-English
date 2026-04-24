namespace EnglishCoach.Contracts.Shared;

public record ApiErrorResponse(
    string Code,
    string Message,
    string? Details = null
);

public record PaginatedRequest(
    int Page = 1,
    int PageSize = 20
);

public record IdRequest(Guid Id);