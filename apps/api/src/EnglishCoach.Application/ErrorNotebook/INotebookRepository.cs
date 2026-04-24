using EnglishCoach.Domain.ErrorNotebook;

namespace EnglishCoach.Application.ErrorNotebook;

public interface INotebookRepository
{
    Task<NotebookEntry?> GetByPatternKeyAsync(string learnerId, string patternKey, CancellationToken ct = default);
    Task<IReadOnlyList<NotebookEntry>> GetLearnerEntriesAsync(string learnerId, CancellationToken ct = default);
    Task CreateAsync(NotebookEntry entry, CancellationToken ct = default);
    Task UpdateAsync(NotebookEntry entry, CancellationToken ct = default);
}

public interface IReviewIntegrationService
{
    Task EnsureReviewItemExistsAsync(string learnerId, string patternKey, CancellationToken ct = default);
}
