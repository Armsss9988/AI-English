using EnglishCoach.Domain.Roleplay;

namespace EnglishCoach.UnitTests.Roleplay;

/// <summary>
/// RP4 — Tests for RecordTurn and FinalizeRoleplay domain behavior.
/// Use case tests focus on the domain invariants since the use cases
/// delegate to domain methods after persisting.
/// </summary>
public class RecordTurnAndFinalizeTests
{
    // ── RP4 Acceptance: Learner message is persisted before generating next reply ──
    // This is an infrastructure concern tested at use-case level.
    // Domain invariant: learner turn must be accepted before session continues.

    [Fact]
    public void AddLearnerTurn_InActiveSession_Succeeds()
    {
        var session = CreateActiveSession();
        session.AddLearnerTurn("I will check with the team.");
        Assert.Equal(2, session.Turns.Count); // 1 client + 1 learner
    }

    [Fact]
    public void AddLearnerTurn_InCreatedSession_Throws()
    {
        var session = RoleplaySession.Create("s1", "l1", "sc1", 1);
        Assert.Throws<InvalidOperationException>(() =>
            session.AddLearnerTurn("Too early"));
    }

    // ── RP4 Acceptance: Finalization requires at least one learner turn ──

    [Fact]
    public void Finalize_WithoutLearnerTurn_Throws()
    {
        var session = RoleplaySession.Create("s1", "l1", "sc1", 1);
        session.AddClientTurn("Hello");
        // No learner turn

        Assert.Throws<InvalidOperationException>(() => session.RequestFeedback());
    }

    [Fact]
    public void Finalize_WithLearnerTurn_Succeeds()
    {
        var session = CreateActiveSession();
        session.AddLearnerTurn("Hi");
        session.RequestFeedback();
        session.Finalize(CreateSummary());

        Assert.Equal(RoleplaySessionState.Finalized, session.State);
    }

    // ── RP4 Acceptance: Session cannot be finalized twice ──

    [Fact]
    public void Finalize_Twice_Throws()
    {
        var session = CreateActiveSession();
        session.AddLearnerTurn("Hi");
        session.RequestFeedback();
        session.Finalize(CreateSummary());

        Assert.Throws<InvalidOperationException>(() => session.Finalize(CreateSummary()));
    }

    // ── RP4 Acceptance: Summary includes result, clear points, top mistakes,
    //    improved answer, phrases to review, retry challenge ──

    [Fact]
    public void Summary_ContainsAllRequiredFields()
    {
        var session = CreateActiveSession();
        session.AddLearnerTurn("Hi");
        session.RequestFeedback();

        var summary = new RoleplaySummary(
            "Passed",
            "Clear introduction",
            "Watch tense agreement",
            "I will follow up with the client by tomorrow",
            "follow up, sync up",
            "Try a more confident opening"
        );

        session.Finalize(summary);

        Assert.Equal("Passed", session.Summary!.Result);
        Assert.Equal("Clear introduction", session.Summary.ClearPoints);
        Assert.Equal("Watch tense agreement", session.Summary.TopMistakes);
        Assert.Equal("I will follow up with the client by tomorrow", session.Summary.ImprovedAnswer);
        Assert.Equal("follow up, sync up", session.Summary.PhrasesToReview);
        Assert.Equal("Try a more confident opening", session.Summary.RetryChallenge);
    }

    // ── Turn ordering ──

    [Fact]
    public void Turns_PreserveOrder()
    {
        var session = CreateActiveSession();
        session.AddLearnerTurn("Learner 1");
        session.AddClientTurn("Client 2");
        session.AddLearnerTurn("Learner 2");

        Assert.Equal(4, session.Turns.Count); // initial client + 3 more
        Assert.Equal(TurnRole.Client, session.Turns[0].Role);
        Assert.Equal(TurnRole.Learner, session.Turns[1].Role);
        Assert.Equal(TurnRole.Client, session.Turns[2].Role);
        Assert.Equal(TurnRole.Learner, session.Turns[3].Role);
    }

    // ── Helpers ──

    private static RoleplaySession CreateActiveSession()
    {
        var session = RoleplaySession.Create("s1", "l1", "sc1", 1);
        session.AddClientTurn("Hello, I need an update on the project.");
        return session;
    }

    private static RoleplaySummary CreateSummary() =>
        new("Passed", "Good points", "Tense issues", "Better answer", "phrases", "Retry challenge");
}
