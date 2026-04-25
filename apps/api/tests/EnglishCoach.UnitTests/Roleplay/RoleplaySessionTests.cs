using EnglishCoach.Domain.Roleplay;

namespace EnglishCoach.UnitTests.Roleplay;

public class RoleplaySessionTests
{
    private static RoleplaySession CreateTestSession() =>
        RoleplaySession.Create(
            Guid.NewGuid().ToString(),
            "learner-1",
            "scenario-1",
            1);

    // ── RP1 Acceptance: State machine Created → Active → AwaitingFeedback → Finalized → Archived ──

    [Fact]
    public void Create_StartsInCreatedState()
    {
        var session = CreateTestSession();
        Assert.Equal(RoleplaySessionState.Created, session.State);
    }

    [Fact]
    public void AddClientTurn_FromCreated_TransitionsToActive()
    {
        var session = CreateTestSession();
        session.AddClientTurn("Hello, I need help.");
        Assert.Equal(RoleplaySessionState.Active, session.State);
    }

    [Fact]
    public void RequestFeedback_FromActive_TransitionsToAwaitingFeedback()
    {
        var session = CreateTestSession();
        session.AddClientTurn("Hello");
        session.AddLearnerTurn("Hi, how can I help?");
        session.RequestFeedback();
        Assert.Equal(RoleplaySessionState.AwaitingFeedback, session.State);
    }

    [Fact]
    public void Finalize_FromAwaitingFeedback_TransitionsToFinalized()
    {
        var session = CreateTestSession();
        session.AddClientTurn("Hello");
        session.AddLearnerTurn("Hi there");
        session.RequestFeedback();
        session.Finalize(CreateTestSummary());
        Assert.Equal(RoleplaySessionState.Finalized, session.State);
    }

    [Fact]
    public void Archive_FromFinalized_TransitionsToArchived()
    {
        var session = CreateTestSession();
        session.AddClientTurn("Hello");
        session.AddLearnerTurn("Hi");
        session.RequestFeedback();
        session.Finalize(CreateTestSummary());
        session.Archive();
        Assert.Equal(RoleplaySessionState.Archived, session.State);
    }

    // ── RP1 Acceptance: Every turn belongs to one session ──

    [Fact]
    public void Turns_AllBelongToSession()
    {
        var session = CreateTestSession();
        session.AddClientTurn("Hello");
        session.AddLearnerTurn("Hi");

        Assert.All(session.Turns, t => Assert.Equal(session.Id, t.SessionId));
    }

    // ── RP1 Acceptance: Only active sessions accept learner turns ──

    [Fact]
    public void AddLearnerTurn_FromCreated_Throws()
    {
        var session = CreateTestSession();
        Assert.Throws<InvalidOperationException>(() =>
            session.AddLearnerTurn("Too early"));
    }

    [Fact]
    public void AddLearnerTurn_FromFinalized_Throws()
    {
        var session = CreateTestSession();
        session.AddClientTurn("Hello");
        session.AddLearnerTurn("Hi");
        session.RequestFeedback();
        session.Finalize(CreateTestSummary());

        Assert.Throws<InvalidOperationException>(() =>
            session.AddLearnerTurn("Too late"));
    }

    // ── RP1 Acceptance: Session cannot be finalized twice ──

    [Fact]
    public void Finalize_Twice_Throws()
    {
        var session = CreateTestSession();
        session.AddClientTurn("Hello");
        session.AddLearnerTurn("Hi");
        session.RequestFeedback();
        session.Finalize(CreateTestSummary());

        Assert.Throws<InvalidOperationException>(() =>
            session.Finalize(CreateTestSummary()));
    }

    // ── RP1 Acceptance: Summary cannot exist without at least one learner turn ──

    [Fact]
    public void RequestFeedback_WithoutLearnerTurn_Throws()
    {
        var session = CreateTestSession();
        session.AddClientTurn("Hello");

        Assert.Throws<InvalidOperationException>(() => session.RequestFeedback());
    }

    // ── Stores scenario content version ──

    [Fact]
    public void Create_StoresScenarioContentVersion()
    {
        var session = RoleplaySession.Create("s1", "learner", "scenario", 3);
        Assert.Equal(3, session.ScenarioContentVersion);
    }

    // ── Summary is stored ──

    [Fact]
    public void Finalize_StoresSummary()
    {
        var session = CreateTestSession();
        session.AddClientTurn("Hello");
        session.AddLearnerTurn("Hi");
        session.RequestFeedback();

        var summary = CreateTestSummary();
        session.Finalize(summary);

        Assert.NotNull(session.Summary);
        Assert.Equal("Passed", session.Summary.Result);
    }

    // ── Null summary throws ──

    [Fact]
    public void Finalize_WithNullSummary_Throws()
    {
        var session = CreateTestSession();
        session.AddClientTurn("Hello");
        session.AddLearnerTurn("Hi");
        session.RequestFeedback();

        Assert.Throws<ArgumentNullException>(() => session.Finalize(null!));
    }

    private static RoleplaySummary CreateTestSummary() =>
        new("Passed", "Good greeting", "Watch tense", "Better answer", "phrase A", "Try intro again");
}
