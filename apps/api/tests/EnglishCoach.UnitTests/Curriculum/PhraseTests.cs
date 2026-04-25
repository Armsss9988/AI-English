using EnglishCoach.Domain.Curriculum;

namespace EnglishCoach.UnitTests.Curriculum;

public class PhraseTests
{
    private static Phrase CreateTestPhrase() =>
        Phrase.Create(
            Guid.NewGuid().ToString(),
            "Let's sync up on this.",
            "Hãy đồng bộ về vấn đề này.",
            CommunicationFunction.Standup,
            ContentLevel.Core,
            "Let's sync up on this task before the sprint ends.");

    // ── C1 Acceptance: Content version is required ──

    [Fact]
    public void Create_SetsContentVersionToOne()
    {
        var phrase = CreateTestPhrase();
        Assert.Equal(1, phrase.ContentVersion);
    }

    [Fact]
    public void Create_StartsInDraftState()
    {
        var phrase = CreateTestPhrase();
        Assert.Equal(ContentPublicationState.Draft, phrase.State);
    }

    [Fact]
    public void Create_IsPublished_ReturnsFalse()
    {
        var phrase = CreateTestPhrase();
        Assert.False(phrase.IsPublished);
    }

    // ── C1 Acceptance: State machine Draft → Review → Published → Deprecated → Archived ──

    [Fact]
    public void SubmitForReview_FromDraft_TransitionsToReview()
    {
        var phrase = CreateTestPhrase();
        phrase.SubmitForReview();
        Assert.Equal(ContentPublicationState.Review, phrase.State);
    }

    [Fact]
    public void Publish_FromReview_TransitionsToPublished()
    {
        var phrase = CreateTestPhrase();
        phrase.SubmitForReview();
        phrase.Publish();
        Assert.Equal(ContentPublicationState.Published, phrase.State);
        Assert.True(phrase.IsPublished);
    }

    [Fact]
    public void Deprecate_FromPublished_TransitionsToDeprecated()
    {
        var phrase = CreateTestPhrase();
        phrase.SubmitForReview();
        phrase.Publish();
        phrase.Deprecate();
        Assert.Equal(ContentPublicationState.Deprecated, phrase.State);
    }

    [Fact]
    public void Archive_FromDeprecated_TransitionsToArchived()
    {
        var phrase = CreateTestPhrase();
        phrase.SubmitForReview();
        phrase.Publish();
        phrase.Deprecate();
        phrase.Archive();
        Assert.Equal(ContentPublicationState.Archived, phrase.State);
    }

    // ── C1 Acceptance: Invalid transitions are rejected ──

    [Fact]
    public void SubmitForReview_FromPublished_Throws()
    {
        var phrase = CreateTestPhrase();
        phrase.SubmitForReview();
        phrase.Publish();
        Assert.Throws<InvalidOperationException>(() => phrase.SubmitForReview());
    }

    [Fact]
    public void Publish_FromDraft_Throws()
    {
        var phrase = CreateTestPhrase();
        Assert.Throws<InvalidOperationException>(() => phrase.Publish());
    }

    [Fact]
    public void Deprecate_FromDraft_Throws()
    {
        var phrase = CreateTestPhrase();
        Assert.Throws<InvalidOperationException>(() => phrase.Deprecate());
    }

    [Fact]
    public void Archive_FromPublished_Throws()
    {
        var phrase = CreateTestPhrase();
        phrase.SubmitForReview();
        phrase.Publish();
        Assert.Throws<InvalidOperationException>(() => phrase.Archive());
    }

    // ── Content version increments ──

    [Fact]
    public void IncrementVersion_IncreasesVersionNumber()
    {
        var phrase = CreateTestPhrase();
        phrase.IncrementVersion("Updated text", "Updated meaning", "Updated example");
        Assert.Equal(2, phrase.ContentVersion);
    }

    [Fact]
    public void IncrementVersion_OnArchived_Throws()
    {
        var phrase = CreateTestPhrase();
        phrase.SubmitForReview();
        phrase.Publish();
        phrase.Deprecate();
        phrase.Archive();
        Assert.Throws<InvalidOperationException>(() =>
            phrase.IncrementVersion("x", "y", "z"));
    }

    // ── C1 Acceptance: Content tables contain no learner progress ──
    // Verified structurally: Phrase entity has no progress fields.

    // ── C1 Acceptance: Draft/unpublished content is not returned to learner queries ──

    [Fact]
    public void Draft_IsPublished_ReturnsFalse()
    {
        var phrase = CreateTestPhrase();
        Assert.False(phrase.IsPublished);
    }

    [Fact]
    public void Review_IsPublished_ReturnsFalse()
    {
        var phrase = CreateTestPhrase();
        phrase.SubmitForReview();
        Assert.False(phrase.IsPublished);
    }

    // ── Required fields ──

    [Fact]
    public void Create_WithEmptyText_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            Phrase.Create("id1", "", "meaning", CommunicationFunction.Standup, ContentLevel.Core, "example"));
    }

    [Fact]
    public void Create_WithEmptyViMeaning_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            Phrase.Create("id1", "text", "", CommunicationFunction.Standup, ContentLevel.Core, "example"));
    }
}
