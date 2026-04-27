using EnglishCoach.Domain.InterviewPractice;
using Xunit;

namespace EnglishCoach.UnitTests.InterviewPractice;

public class InterviewSessionTests
{
    [Fact]
    public void Create_ValidInput_InitializesCorrectly()
    {
        // Act
        var session = InterviewSession.Create(
            "session-1",
            "learner-1",
            "profile-1",
            "JD details",
            InterviewType.Mixed);

        // Assert
        Assert.Equal("session-1", session.Id);
        Assert.Equal("learner-1", session.LearnerId);
        Assert.Equal("profile-1", session.InterviewProfileId);
        Assert.Equal("JD details", session.JdText);
        Assert.Equal(InterviewType.Mixed, session.Type);
        Assert.Equal(InterviewSessionState.Created, session.State);
        Assert.Empty(session.Turns);
    }

    [Fact]
    public void StartAnalysis_FromCreatedState_TransitionsToAnalyzing()
    {
        // Arrange
        var session = InterviewSession.Create("session-1", "learner-1", "profile-1", "JD", InterviewType.Mixed);

        // Act
        session.StartAnalysis();

        // Assert
        Assert.Equal(InterviewSessionState.Analyzing, session.State);
    }

    [Fact]
    public void StartAnalysis_FromInvalidState_ThrowsException()
    {
        // Arrange
        var session = InterviewSession.Create("session-1", "learner-1", "profile-1", "JD", InterviewType.Mixed);
        session.StartAnalysis();
        session.CompleteAnalysis("jd-analysis", "plan", 5); // Now in Ready state

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => session.StartAnalysis());
    }

    [Fact]
    public void CompleteAnalysis_FromAnalyzingState_TransitionsToReady()
    {
        // Arrange
        var session = InterviewSession.Create("session-1", "learner-1", "profile-1", "JD", InterviewType.Mixed);
        session.StartAnalysis();

        // Act
        session.CompleteAnalysis("JD analysis", "Interview Plan", 5);

        // Assert
        Assert.Equal(InterviewSessionState.Ready, session.State);
        Assert.Equal("JD analysis", session.JdAnalysis);
        Assert.Equal("Interview Plan", session.InterviewPlan);
        Assert.Equal(5, session.PlannedQuestionCount);
    }

    [Fact]
    public void AddInterviewerTurn_FromReadyState_TransitionsToActive()
    {
        // Arrange
        var session = InterviewSession.Create("session-1", "learner-1", "profile-1", "JD", InterviewType.Mixed);
        session.StartAnalysis();
        session.CompleteAnalysis("JD analysis", "plan", 5);

        // Act
        session.AddInterviewerTurn("Tell me about yourself.", InterviewQuestionCategory.Opening);

        // Assert
        Assert.Equal(InterviewSessionState.Active, session.State);
        Assert.Single(session.Turns);
        Assert.Equal(InterviewTurnRole.Interviewer, session.Turns[0].Role);
        Assert.Equal("Tell me about yourself.", session.Turns[0].Message);
    }

    [Fact]
    public void AddLearnerTurn_FromActiveState_AddsTurnSuccessfully()
    {
        // Arrange
        var session = InterviewSession.Create("session-1", "learner-1", "profile-1", "JD", InterviewType.Mixed);
        session.StartAnalysis();
        session.CompleteAnalysis("JD analysis", "plan", 5);
        session.AddInterviewerTurn("Tell me about yourself.", InterviewQuestionCategory.Opening);

        // Act
        session.AddLearnerTurn("I am a software engineer.", "http://audio.url");

        // Assert
        Assert.Equal(InterviewSessionState.Active, session.State);
        Assert.Equal(2, session.Turns.Count);
        Assert.Equal(InterviewTurnRole.Learner, session.Turns[1].Role);
        Assert.Equal("I am a software engineer.", session.Turns[1].Message);
        Assert.Equal(1, session.LearnerAnswerCount);
        Assert.False(session.IsQuestionLimitReached);
    }

    [Fact]
    public void AddLearnerTurn_ReachingLimit_IsQuestionLimitReachedTrue()
    {
        // Arrange
        var session = InterviewSession.Create("session-1", "learner-1", "profile-1", "JD", InterviewType.Mixed);
        session.StartAnalysis();
        session.CompleteAnalysis("JD analysis", "plan", 2); // 2 questions limit
        session.AddInterviewerTurn("Q1", InterviewQuestionCategory.Opening);
        session.AddLearnerTurn("A1");
        session.AddInterviewerTurn("Q2", InterviewQuestionCategory.Behavioral);
        
        // Act
        session.AddLearnerTurn("A2");

        // Assert
        Assert.True(session.IsQuestionLimitReached);
    }

    [Fact]
    public void RequestFeedback_FromActiveState_TransitionsToAwaitingFeedback()
    {
        // Arrange
        var session = InterviewSession.Create("session-1", "learner-1", "profile-1", "JD", InterviewType.Mixed);
        session.StartAnalysis();
        session.CompleteAnalysis("JD analysis", "plan", 5);
        session.AddInterviewerTurn("Q1", InterviewQuestionCategory.Opening);
        session.AddLearnerTurn("A1");

        // Act
        session.RequestFeedback();

        // Assert
        Assert.Equal(InterviewSessionState.AwaitingFeedback, session.State);
    }

    [Fact]
    public void SetFeedback_FromAwaitingFeedback_TransitionsToFinalized()
    {
        // Arrange
        var session = InterviewSession.Create("session-1", "learner-1", "profile-1", "JD", InterviewType.Mixed);
        session.StartAnalysis();
        session.CompleteAnalysis("JD analysis", "plan", 5);
        session.AddInterviewerTurn("Q1", InterviewQuestionCategory.Opening);
        session.AddLearnerTurn("A1");
        session.RequestFeedback();

        var feedback = new InterviewFeedback(
            80, 80, 80, 80, "Good EN", "Good VI", new List<string>(), new List<string>(), new List<string>(), "Try again");

        // Act
        session.SetFeedback(feedback);

        // Assert
        Assert.Equal(InterviewSessionState.Finalized, session.State);
        Assert.NotNull(session.Feedback);
        Assert.Equal(80, session.Feedback.OverallScore);
    }
}
