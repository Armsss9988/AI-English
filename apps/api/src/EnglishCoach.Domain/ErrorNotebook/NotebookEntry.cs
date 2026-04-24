namespace EnglishCoach.Domain.ErrorNotebook;

public enum ErrorCategory
{
    Clarity,
    Grammar,
    Phrase,
    Pronunciation,
    Tone
}

public enum ErrorSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum NotebookEntryState
{
    New,
    Learning,
    Stable,
    Archived
}

public sealed record NotebookEvidence(
    string SourceAttemptId,
    string Context,
    DateTimeOffset RecordedAtUtc
);

/// <summary>
/// E1: Store recurring learner mistakes with evidence.
/// </summary>
public sealed class NotebookEntry
{
    private NotebookEntry()
    {
        Id = string.Empty;
        LearnerId = string.Empty;
        PatternKey = string.Empty;
        OriginalExample = string.Empty;
        CorrectedExample = string.Empty;
        ExplanationVi = string.Empty;
    }

    public string Id { get; private set; }
    public string LearnerId { get; private set; }
    public string PatternKey { get; private set; }
    public ErrorCategory Category { get; private set; }
    public ErrorSeverity Severity { get; private set; }
    
    public string OriginalExample { get; private set; }
    public string CorrectedExample { get; private set; }
    public string ExplanationVi { get; private set; }
    
    public int RecurrenceCount { get; private set; }
    public NotebookEntryState State { get; private set; }

    private readonly List<NotebookEvidence> _evidenceRefs = new();
    public IReadOnlyList<NotebookEvidence> EvidenceRefs => _evidenceRefs.AsReadOnly();

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static NotebookEntry Create(
        string id,
        string learnerId,
        string patternKey,
        ErrorCategory category,
        ErrorSeverity severity,
        string originalExample,
        string correctedExample,
        string explanationVi,
        NotebookEvidence initialEvidence)
    {
        var entry = new NotebookEntry
        {
            Id = RequireNonEmpty(id, nameof(id)),
            LearnerId = RequireNonEmpty(learnerId, nameof(learnerId)),
            PatternKey = RequireNonEmpty(patternKey, nameof(patternKey)),
            Category = category,
            Severity = severity,
            OriginalExample = RequireNonEmpty(originalExample, nameof(originalExample)),
            CorrectedExample = RequireNonEmpty(correctedExample, nameof(correctedExample)),
            ExplanationVi = RequireNonEmpty(explanationVi, nameof(explanationVi)),
            RecurrenceCount = 1,
            State = NotebookEntryState.New,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        entry._evidenceRefs.Add(initialEvidence);

        return entry;
    }

    public void RecordRecurrence(NotebookEvidence evidence)
    {
        if (State == NotebookEntryState.Archived)
            throw new InvalidOperationException("Cannot record recurrence for an archived entry.");

        _evidenceRefs.Add(evidence);
        RecurrenceCount++;
        
        if (State == NotebookEntryState.Stable || State == NotebookEntryState.New)
        {
            State = NotebookEntryState.Learning;
        }

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkAsStable()
    {
        if (State == NotebookEntryState.Archived)
            throw new InvalidOperationException("Archived entry cannot be marked as stable.");
        
        State = NotebookEntryState.Stable;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        State = NotebookEntryState.Archived;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string RequireNonEmpty(string value, string paramName) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", paramName) : value.Trim();
}
