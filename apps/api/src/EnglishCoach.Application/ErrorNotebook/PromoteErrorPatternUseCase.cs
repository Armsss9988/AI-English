using EnglishCoach.Domain.ErrorNotebook;

namespace EnglishCoach.Application.ErrorNotebook;

public record PromoteErrorPatternRequest(
    string PatternKey,
    ErrorCategory Category,
    ErrorSeverity Severity,
    string OriginalExample,
    string CorrectedExample,
    string ExplanationVi,
    string SourceAttemptId,
    string Context
);

public sealed class PromoteErrorPatternUseCase
{
    private readonly INotebookRepository _repository;
    private readonly IReviewIntegrationService _reviewIntegration;

    public PromoteErrorPatternUseCase(
        INotebookRepository repository,
        IReviewIntegrationService reviewIntegration)
    {
        _repository = repository;
        _reviewIntegration = reviewIntegration;
    }

    public async Task<string> ExecuteAsync(
        string learnerId,
        PromoteErrorPatternRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(learnerId))
            throw new ArgumentException("Learner ID is required.");

        var evidence = new NotebookEvidence(
            request.SourceAttemptId,
            request.Context,
            DateTimeOffset.UtcNow
        );

        var existingEntry = await _repository.GetByPatternKeyAsync(learnerId, request.PatternKey, ct);

        string entryId;

        if (existingEntry is not null)
        {
            existingEntry.RecordRecurrence(evidence);
            await _repository.UpdateAsync(existingEntry, ct);
            entryId = existingEntry.Id;
        }
        else
        {
            entryId = Guid.NewGuid().ToString();
            var newEntry = NotebookEntry.Create(
                entryId,
                learnerId,
                request.PatternKey,
                request.Category,
                request.Severity,
                request.OriginalExample,
                request.CorrectedExample,
                request.ExplanationVi,
                evidence
            );
            await _repository.CreateAsync(newEntry, ct);
        }

        // Schedule review
        await _reviewIntegration.EnsureReviewItemExistsAsync(learnerId, request.PatternKey, ct);

        return entryId;
    }
}
