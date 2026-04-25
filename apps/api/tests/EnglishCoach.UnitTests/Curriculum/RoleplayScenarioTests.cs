using EnglishCoach.Domain.Curriculum;

namespace EnglishCoach.UnitTests.Curriculum;

public class RoleplayScenarioTests
{
    private static RoleplayScenario CreateTestScenario() =>
        RoleplayScenario.Create(
            Guid.NewGuid().ToString(),
            "Daily Standup Update",
            "Agile team standup meeting",
            "Developer",
            "Senior project manager",
            "Report progress and blockers clearly",
            new[] { "Yesterday's progress", "Today's plan", "Blockers" },
            new[] { "phrase-1", "phrase-2" },
            new[] { "Polite greeting", "Clear status", "Mention blockers" },
            3);

    // ── C2 Acceptance: Scenario can be loaded by id ──

    [Fact]
    public void Create_SetsAllFields()
    {
        var scenario = CreateTestScenario();

        Assert.Equal("Daily Standup Update", scenario.Title);
        Assert.Equal("Agile team standup meeting", scenario.WorkplaceContext);
        Assert.Equal("Developer", scenario.UserRole);
        Assert.Equal("Senior project manager", scenario.ClientPersona);
        Assert.Equal("Report progress and blockers clearly", scenario.CommunicationGoal);
        Assert.Equal(3, scenario.Difficulty);
    }

    // ── C2 Acceptance: Scenario references target phrases explicitly ──

    [Fact]
    public void Create_StoresTargetPhraseIds()
    {
        var scenario = CreateTestScenario();
        Assert.Equal(2, scenario.TargetPhraseIds.Count);
        Assert.Contains("phrase-1", scenario.TargetPhraseIds);
        Assert.Contains("phrase-2", scenario.TargetPhraseIds);
    }

    // ── C2 Acceptance: Pass criteria are stored as data, not prompt-only text ──

    [Fact]
    public void Create_StoresPassCriteriaAsList()
    {
        var scenario = CreateTestScenario();
        Assert.Equal(3, scenario.PassCriteria.Count);
        Assert.Contains("Clear status", scenario.PassCriteria);
    }

    [Fact]
    public void Create_StoresMustCoverPoints()
    {
        var scenario = CreateTestScenario();
        Assert.Equal(3, scenario.MustCoverPoints.Count);
        Assert.Contains("Blockers", scenario.MustCoverPoints);
    }

    // ── C2 Acceptance: Content version is preserved ──

    [Fact]
    public void Create_SetsContentVersionToOne()
    {
        var scenario = CreateTestScenario();
        Assert.Equal(1, scenario.ContentVersion);
    }

    // ── State machine: same as Phrase ──

    [Fact]
    public void Create_StartsInDraft()
    {
        var scenario = CreateTestScenario();
        Assert.Equal(ContentPublicationState.Draft, scenario.State);
    }

    [Fact]
    public void FullLifecycle_DraftToArchived()
    {
        var scenario = CreateTestScenario();
        scenario.SubmitForReview();
        Assert.Equal(ContentPublicationState.Review, scenario.State);

        scenario.Publish();
        Assert.Equal(ContentPublicationState.Published, scenario.State);
        Assert.True(scenario.IsPublished);

        scenario.Deprecate();
        Assert.Equal(ContentPublicationState.Deprecated, scenario.State);

        scenario.Archive();
        Assert.Equal(ContentPublicationState.Archived, scenario.State);
    }

    [Fact]
    public void Publish_FromDraft_Throws()
    {
        var scenario = CreateTestScenario();
        Assert.Throws<InvalidOperationException>(() => scenario.Publish());
    }

    [Fact]
    public void SubmitForReview_FromPublished_Throws()
    {
        var scenario = CreateTestScenario();
        scenario.SubmitForReview();
        scenario.Publish();
        Assert.Throws<InvalidOperationException>(() => scenario.SubmitForReview());
    }

    // ── Required fields ──

    [Fact]
    public void Create_WithEmptyTitle_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            RoleplayScenario.Create("id", "", "context", "role", "persona", "goal",
                Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), 1));
    }

    [Fact]
    public void Create_WithEmptyGoal_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            RoleplayScenario.Create("id", "title", "context", "role", "persona", "",
                Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), 1));
    }
}
