using EnglishCoach.Domain.ErrorNotebook;
using EnglishCoach.Domain.Review;
using EnglishCoach.Application.Review;
using EnglishCoach.SharedKernel.Time;

namespace EnglishCoach.Application.ErrorNotebook;

public record NotebookEntryResponse(
    string Id,
    string PatternKey,
    string Category,
    string Severity,
    string OriginalExample,
    string CorrectedExample,
    string ExplanationVi,
    int RecurrenceCount,
    string State,
    bool IsDue
);

public sealed class GetNotebookEntriesUseCase
{
    private readonly INotebookRepository _notebookRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IClock _clock;

    public GetNotebookEntriesUseCase(
        INotebookRepository notebookRepository,
        IReviewRepository reviewRepository,
        IClock clock)
    {
        _notebookRepository = notebookRepository;
        _reviewRepository = reviewRepository;
        _clock = clock;
    }

    public async Task<IReadOnlyList<NotebookEntryResponse>> ExecuteAsync(string learnerId, CancellationToken ct = default)
    {
        var entries = await _notebookRepository.GetLearnerEntriesAsync(learnerId, ct);
        var activeEntries = entries.Where(e => e.State != NotebookEntryState.Archived).ToList();

        var responses = new List<NotebookEntryResponse>();
        var now = _clock.UtcNow;

        foreach (var entry in activeEntries)
        {
            // Notebook pattern key is mapped to ReviewItemId directly in the promotion use case
            var reviewItem = await _reviewRepository.GetByCompositeKeyAsync(learnerId, entry.PatternKey, ReviewTrack.Error, ct);
            
            bool isDue = reviewItem?.IsDue(now) ?? false;

            responses.Add(new NotebookEntryResponse(
                entry.Id,
                entry.PatternKey,
                entry.Category.ToString(),
                entry.Severity.ToString(),
                entry.OriginalExample,
                entry.CorrectedExample,
                entry.ExplanationVi,
                entry.RecurrenceCount,
                entry.State.ToString(),
                isDue
            ));
        }

        // Sort by Due (true first), then Severity (Critical first), then Recurrence (desc)
        return responses
            .OrderByDescending(r => r.IsDue)
            .ThenByDescending(r => Enum.Parse<ErrorSeverity>(r.Severity))
            .ThenByDescending(r => r.RecurrenceCount)
            .ToList()
            .AsReadOnly();
    }
}
