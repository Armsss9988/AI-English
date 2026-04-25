using EnglishCoach.Domain.Speaking;

namespace EnglishCoach.UnitTests.Speaking;

public class SpeakingAttemptTests
{
    private static SpeakingAttempt CreateTestAttempt() =>
        SpeakingAttempt.Create(
            Guid.NewGuid().ToString("N"),
            "learner-1",
            "content-item-1");

    // ── S1 Acceptance: State machine Created → Uploaded → Transcribed → Evaluated → Finalized ──

    [Fact]
    public void Create_StartsInCreatedState()
    {
        var attempt = CreateTestAttempt();
        Assert.Equal(SpeakingAttemptState.Created, attempt.State);
    }

    [Fact]
    public void MarkUploaded_FromCreated_TransitionsToUploaded()
    {
        var attempt = CreateTestAttempt();
        attempt.MarkUploaded("https://audio.example.com/test.wav");
        Assert.Equal(SpeakingAttemptState.Uploaded, attempt.State);
    }

    [Fact]
    public void MarkTranscribed_FromUploaded_TransitionsToTranscribed()
    {
        var attempt = CreateTestAttempt();
        attempt.MarkUploaded("https://audio.example.com/test.wav");
        attempt.MarkTranscribed("raw transcript", "normalized transcript");
        Assert.Equal(SpeakingAttemptState.Transcribed, attempt.State);
    }

    [Fact]
    public void MarkEvaluated_FromTranscribed_TransitionsToEvaluated()
    {
        var attempt = CreateTestAttempt();
        attempt.MarkTranscribed("raw", "normalized");
        var feedback = new SpeakingFeedback("tense error", "better answer", "phrase A", "try again");
        attempt.MarkEvaluated(feedback);
        Assert.Equal(SpeakingAttemptState.Evaluated, attempt.State);
        Assert.NotNull(attempt.Feedback);
    }

    [Fact]
    public void FinalizeAttempt_FromEvaluated_TransitionsToFinalized()
    {
        var attempt = CreateTestAttempt();
        attempt.MarkTranscribed("raw", "normalized");
        attempt.MarkEvaluated(new SpeakingFeedback("m", "i", "p", "r"));
        attempt.FinalizeAttempt();
        Assert.Equal(SpeakingAttemptState.Finalized, attempt.State);
    }

    // ── S1 Acceptance: MVP path — Created → Transcribed (skip upload) ──

    [Fact]
    public void MarkTranscribed_FromCreated_TransitionsToTranscribed()
    {
        var attempt = CreateTestAttempt();
        attempt.MarkTranscribed("text input", "text input");
        Assert.Equal(SpeakingAttemptState.Transcribed, attempt.State);
    }

    // ── S1 Acceptance: Invalid transitions are rejected ──

    [Fact]
    public void MarkUploaded_FromTranscribed_Throws()
    {
        var attempt = CreateTestAttempt();
        attempt.MarkTranscribed("raw", "norm");
        Assert.Throws<InvalidOperationException>(() =>
            attempt.MarkUploaded("https://audio.example.com/test.wav"));
    }

    [Fact]
    public void MarkEvaluated_FromCreated_Throws()
    {
        var attempt = CreateTestAttempt();
        Assert.Throws<InvalidOperationException>(() =>
            attempt.MarkEvaluated(new SpeakingFeedback("m", "i", "p", "r")));
    }

    [Fact]
    public void FinalizeAttempt_FromCreated_Throws()
    {
        var attempt = CreateTestAttempt();
        Assert.Throws<InvalidOperationException>(() => attempt.FinalizeAttempt());
    }

    [Fact]
    public void FinalizeAttempt_FromTranscribed_Throws()
    {
        var attempt = CreateTestAttempt();
        attempt.MarkTranscribed("raw", "norm");
        Assert.Throws<InvalidOperationException>(() => attempt.FinalizeAttempt());
    }

    // ── S1 Acceptance: Attempt belongs to one user and one content item ──

    [Fact]
    public void Create_SetsOwnerAndContentItem()
    {
        var attempt = SpeakingAttempt.Create("attempt-id", "user-abc", "content-xyz");
        Assert.Equal("user-abc", attempt.LearnerId);
        Assert.Equal("content-xyz", attempt.ContentItemId);
    }

    // ── S1 Acceptance: Raw transcript and normalized transcript are separate concepts ──

    [Fact]
    public void MarkTranscribed_StoresBothTranscriptsSeparately()
    {
        var attempt = CreateTestAttempt();
        attempt.MarkTranscribed("raw with uh um", "clean normalized");
        Assert.Equal("raw with uh um", attempt.RawTranscript);
        Assert.Equal("clean normalized", attempt.NormalizedTranscript);
    }

    // ── S1 Acceptance: Domain has no provider-specific fields ──
    // Verified structurally: SpeakingAttempt only has domain fields.

    // ── Required fields ──

    [Fact]
    public void Create_WithEmptyId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            SpeakingAttempt.Create("", "learner", "content"));
    }

    [Fact]
    public void Create_WithEmptyLearnerId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            SpeakingAttempt.Create("id", "", "content"));
    }

    [Fact]
    public void MarkEvaluated_WithNullFeedback_Throws()
    {
        var attempt = CreateTestAttempt();
        attempt.MarkTranscribed("raw", "norm");
        Assert.Throws<ArgumentNullException>(() => attempt.MarkEvaluated(null!));
    }
}
