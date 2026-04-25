using EnglishCoach.Domain.ErrorNotebook;

namespace EnglishCoach.UnitTests.ErrorNotebook;

public class NotebookEntryTests
{
    // ── E1 Acceptance: Entry belongs to one learner ──

    [Fact]
    public void Create_SetsLearnerOwnership()
    {
        var entry = CreateTestEntry("learner-A");
        Assert.Equal("learner-A", entry.LearnerId);
    }

    // ── E1 Acceptance: Evidence is retained during merges ──

    [Fact]
    public void RecordRecurrence_AppendsEvidence()
    {
        var entry = CreateTestEntry("learner-1");
        var newEvidence = new NotebookEvidence("attempt-2", "Second occurrence", DateTimeOffset.UtcNow);

        entry.RecordRecurrence(newEvidence);

        Assert.Equal(2, entry.EvidenceRefs.Count);
        Assert.Contains(entry.EvidenceRefs, e => e.SourceAttemptId == "attempt-2");
    }

    // ── E1 Acceptance: Duplicate pattern merge rules are pure and tested ──

    [Fact]
    public void RecordRecurrence_IncrementsRecurrenceCount()
    {
        var entry = CreateTestEntry("learner-1");
        Assert.Equal(1, entry.RecurrenceCount);

        entry.RecordRecurrence(new NotebookEvidence("a2", "ctx", DateTimeOffset.UtcNow));
        Assert.Equal(2, entry.RecurrenceCount);

        entry.RecordRecurrence(new NotebookEvidence("a3", "ctx", DateTimeOffset.UtcNow));
        Assert.Equal(3, entry.RecurrenceCount);
    }

    // ── E1 Acceptance: Historical evidence is never overwritten ──

    [Fact]
    public void RecordRecurrence_OriginalEvidencePreserved()
    {
        var entry = CreateTestEntry("learner-1");
        var originalEvidence = entry.EvidenceRefs[0];

        entry.RecordRecurrence(new NotebookEvidence("a2", "new context", DateTimeOffset.UtcNow));

        Assert.Equal(2, entry.EvidenceRefs.Count);
        Assert.Equal(originalEvidence.SourceAttemptId, entry.EvidenceRefs[0].SourceAttemptId);
    }

    // ── Archived entry rejects recurrence ──

    [Fact]
    public void RecordRecurrence_OnArchived_Throws()
    {
        var entry = CreateTestEntry("learner-1");
        entry.Archive();

        Assert.Throws<InvalidOperationException>(() =>
            entry.RecordRecurrence(new NotebookEvidence("a2", "ctx", DateTimeOffset.UtcNow)));
    }

    // ── State transitions ──

    [Fact]
    public void Create_StartsInNewState()
    {
        var entry = CreateTestEntry("learner-1");
        Assert.Equal(NotebookEntryState.New, entry.State);
    }

    [Fact]
    public void RecordRecurrence_FromNew_TransitionsToLearning()
    {
        var entry = CreateTestEntry("learner-1");
        entry.RecordRecurrence(new NotebookEvidence("a2", "ctx", DateTimeOffset.UtcNow));
        Assert.Equal(NotebookEntryState.Learning, entry.State);
    }

    [Fact]
    public void MarkAsStable_SetsStableState()
    {
        var entry = CreateTestEntry("learner-1");
        entry.MarkAsStable();
        Assert.Equal(NotebookEntryState.Stable, entry.State);
    }

    [Fact]
    public void RecordRecurrence_FromStable_TransitionsBackToLearning()
    {
        var entry = CreateTestEntry("learner-1");
        entry.MarkAsStable();
        Assert.Equal(NotebookEntryState.Stable, entry.State);

        entry.RecordRecurrence(new NotebookEvidence("a2", "ctx", DateTimeOffset.UtcNow));
        Assert.Equal(NotebookEntryState.Learning, entry.State);
    }

    [Fact]
    public void Archive_SetsArchivedState()
    {
        var entry = CreateTestEntry("learner-1");
        entry.Archive();
        Assert.Equal(NotebookEntryState.Archived, entry.State);
    }

    [Fact]
    public void MarkAsStable_OnArchived_Throws()
    {
        var entry = CreateTestEntry("learner-1");
        entry.Archive();
        Assert.Throws<InvalidOperationException>(() => entry.MarkAsStable());
    }

    // ── Required fields ──

    [Fact]
    public void Create_WithEmptyPatternKey_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            NotebookEntry.Create("id", "learner", "", ErrorCategory.Grammar, ErrorSeverity.High,
                "original", "corrected", "explanation",
                new NotebookEvidence("a1", "ctx", DateTimeOffset.UtcNow)));
    }

    [Fact]
    public void Create_StoresAllFields()
    {
        var entry = CreateTestEntry("learner-1");

        Assert.Equal("missing-article", entry.PatternKey);
        Assert.Equal(ErrorCategory.Grammar, entry.Category);
        Assert.Equal(ErrorSeverity.High, entry.Severity);
        Assert.Equal("I go office", entry.OriginalExample);
        Assert.Equal("I go to the office", entry.CorrectedExample);
        Assert.Equal("Thiếu mạo từ 'the'", entry.ExplanationVi);
    }

    // ── Helpers ──

    private static NotebookEntry CreateTestEntry(string learnerId) =>
        NotebookEntry.Create(
            Guid.NewGuid().ToString(),
            learnerId,
            "missing-article",
            ErrorCategory.Grammar,
            ErrorSeverity.High,
            "I go office",
            "I go to the office",
            "Thiếu mạo từ 'the'",
            new NotebookEvidence("attempt-1", "Speaking drill context", DateTimeOffset.UtcNow));
}
