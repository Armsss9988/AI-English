using EnglishCoach.Domain.InterviewPractice;
using Xunit;

namespace EnglishCoach.UnitTests.InterviewPractice;

public sealed class InterviewCapabilityTests
{
    [Theory]
    [InlineData("SelfIntroduction", InterviewCapability.SelfIntroduction)]
    [InlineData("ProjectDeepDive", InterviewCapability.ProjectDeepDive)]
    [InlineData("TechnicalTradeoff", InterviewCapability.TechnicalTradeoff)]
    [InlineData("BehavioralStar", InterviewCapability.BehavioralStar)]
    [InlineData("ClientCommunication", InterviewCapability.ClientCommunication)]
    [InlineData("RequirementClarification", InterviewCapability.RequirementClarification)]
    [InlineData("IncidentConflictStory", InterviewCapability.IncidentConflictStory)]
    [InlineData("WeakSpotRetry", InterviewCapability.WeakSpotRetry)]
    [InlineData("EnglishClarity", InterviewCapability.EnglishClarity)]
    [InlineData("PronunciationClarity", InterviewCapability.PronunciationClarity)]
    public void ParseCapability_ValidValues_Succeeds(string input, InterviewCapability expected)
    {
        var result = InterviewCapabilityExtensions.ParseCapability(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("selfintroduction")]
    [InlineData("SELFINTRODUCTION")]
    [InlineData("selfIntroduction")]
    public void ParseCapability_CaseInsensitive(string input)
    {
        var result = InterviewCapabilityExtensions.ParseCapability(input);
        Assert.Equal(InterviewCapability.SelfIntroduction, result);
    }

    [Theory]
    [InlineData("UnknownCapability")]
    [InlineData("InvalidValue")]
    [InlineData("Random")]
    public void ParseCapability_UnknownValues_Throws(string input)
    {
        Assert.Throws<ArgumentException>(() => InterviewCapabilityExtensions.ParseCapability(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ParseCapability_EmptyOrNull_Throws(string? input)
    {
        Assert.Throws<ArgumentException>(() => InterviewCapabilityExtensions.ParseCapability(input!));
    }

    [Fact]
    public void TryParseCapability_ValidValue_ReturnsTrue()
    {
        var result = InterviewCapabilityExtensions.TryParseCapability("ProjectDeepDive", out var capability);
        Assert.True(result);
        Assert.Equal(InterviewCapability.ProjectDeepDive, capability);
    }

    [Fact]
    public void TryParseCapability_InvalidValue_ReturnsFalse()
    {
        var result = InterviewCapabilityExtensions.TryParseCapability("NotACapability", out _);
        Assert.False(result);
    }

    [Fact]
    public void All_10_Capabilities_Are_Defined()
    {
        var values = Enum.GetValues<InterviewCapability>();
        Assert.Equal(10, values.Length);
    }
}

public sealed class InterviewTurnTypeTests
{
    [Theory]
    [InlineData("OpeningQuestion", InterviewTurnType.OpeningQuestion)]
    [InlineData("MainQuestion", InterviewTurnType.MainQuestion)]
    [InlineData("FollowUp", InterviewTurnType.FollowUp)]
    [InlineData("Clarification", InterviewTurnType.Clarification)]
    [InlineData("Challenge", InterviewTurnType.Challenge)]
    [InlineData("Transition", InterviewTurnType.Transition)]
    [InlineData("Closing", InterviewTurnType.Closing)]
    public void ParseTurnType_ValidValues_Succeeds(string input, InterviewTurnType expected)
    {
        var result = InterviewTurnTypeExtensions.ParseTurnType(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("UnknownType")]
    [InlineData("")]
    public void ParseTurnType_InvalidValues_Throws(string input)
    {
        Assert.Throws<ArgumentException>(() => InterviewTurnTypeExtensions.ParseTurnType(input));
    }

    [Fact]
    public void All_7_TurnTypes_Are_Defined()
    {
        var values = Enum.GetValues<InterviewTurnType>();
        Assert.Equal(7, values.Length);
    }
}

public sealed class InterviewTurnAdaptiveTests
{
    [Fact]
    public void CreateInterviewerTurn_WithRubric_StoresRubricJson()
    {
        var rubric = new InterviewQuestionRubric
        {
            Capability = InterviewCapability.ProjectDeepDive,
            SuccessCriteria = "Candidate describes a concrete project with their role and impact.",
            ExpectedCvEvidence = "Backend API development",
            JdSignals = "RESTful API design",
            AnswerStructureHint = "Use STAR method"
        };

        var turn = InterviewTurn.CreateInterviewerTurn(
            "session-1",
            "Tell me about a project you led.",
            1,
            InterviewTurnType.MainQuestion,
            InterviewCapability.ProjectDeepDive,
            rubric,
            null,
            InterviewVerificationStatus.Verified);

        Assert.Equal(InterviewTurnRole.Interviewer, turn.Role);
        Assert.Equal(InterviewTurnType.MainQuestion, turn.TurnType);
        Assert.Equal(InterviewCapability.ProjectDeepDive, turn.TargetCapability);
        Assert.Equal(InterviewTurnState.Created, turn.TurnState);
        Assert.Equal(InterviewVerificationStatus.Verified, turn.VerificationStatus);
        Assert.False(string.IsNullOrWhiteSpace(turn.RubricJson));

        var parsedRubric = turn.GetRubric();
        Assert.NotNull(parsedRubric);
        Assert.Equal(InterviewCapability.ProjectDeepDive, parsedRubric.Capability);
        Assert.Equal("Use STAR method", parsedRubric.AnswerStructureHint);
    }

    [Fact]
    public void CreateInterviewerTurn_WithDecision_StoresDecisionJson()
    {
        var decision = new InterviewTurnDecision
        {
            TurnType = InterviewTurnType.FollowUp,
            TargetCapability = InterviewCapability.TechnicalTradeoff,
            ReasonCode = "shallow_answer",
            ShouldAdvancePlan = false,
            LearnerFacingHint = "Try to explain why you chose that approach."
        };

        var turn = InterviewTurn.CreateInterviewerTurn(
            "session-1",
            "Can you explain why you chose that approach?",
            2,
            InterviewTurnType.FollowUp,
            InterviewCapability.TechnicalTradeoff,
            null,
            decision,
            InterviewVerificationStatus.Verified);

        var parsedDecision = turn.GetDecision();
        Assert.NotNull(parsedDecision);
        Assert.Equal("shallow_answer", parsedDecision.ReasonCode);
        Assert.False(parsedDecision.ShouldAdvancePlan);
    }

    [Fact]
    public void CreateInterviewerTurn_MapsLegacyCategory()
    {
        var turn = InterviewTurn.CreateInterviewerTurn(
            "session-1",
            "Opening question",
            1,
            InterviewTurnType.OpeningQuestion,
            InterviewCapability.SelfIntroduction,
            null,
            null,
            InterviewVerificationStatus.Verified);

        Assert.Equal(InterviewQuestionCategory.Opening, turn.QuestionCategory);
    }

    [Fact]
    public void CreateLearnerTurn_HasCorrectDefaults()
    {
        var turn = InterviewTurn.CreateLearnerTurn("session-1", "My answer", 2);

        Assert.Equal(InterviewTurnRole.Learner, turn.Role);
        Assert.Equal(InterviewTurnState.Created, turn.TurnState);
        Assert.Equal(InterviewVerificationStatus.Unverified, turn.VerificationStatus);
        Assert.Equal("My answer", turn.Message);
    }

    [Fact]
    public void LearnerTurn_TranscriptConfirmation_FullLifecycle()
    {
        var turn = InterviewTurn.CreateLearnerTurn("session-1", "Original message", 2);

        // Upload audio
        turn.MarkLearnerAudioUploaded("audio/session-1/turn-2.webm", 5000);
        Assert.Equal(InterviewTurnState.LearnerAudioUploaded, turn.TurnState);
        Assert.Equal(5000, turn.AudioDurationMs);

        // STT transcript
        turn.SetTranscript("I work with ASP.NET Core", 0.85);
        Assert.Equal(InterviewTurnState.TranscriptReady, turn.TurnState);
        Assert.Equal(0.85, turn.TranscriptConfidence);

        // Confirm transcript (learner edits)
        turn.ConfirmTranscript("I work with ASP.NET Core and React", learnerEdited: true);
        Assert.Equal(InterviewTurnState.TranscriptConfirmed, turn.TurnState);
        Assert.True(turn.LearnerEditedTranscript);
        Assert.Equal("I work with ASP.NET Core and React", turn.GetEvaluableTranscript());

        // Pronunciation assessed
        turn.SetPronunciationReport("{\"score\": 75}", InterviewVerificationStatus.Verified);
        Assert.Equal(InterviewTurnState.PronunciationAssessed, turn.TurnState);

        // Answer evaluated
        turn.SetScorecard("{\"overall\": 80}", InterviewVerificationStatus.Verified);
        Assert.Equal(InterviewTurnState.AnswerEvaluated, turn.TurnState);
    }

    [Fact]
    public void ConfirmTranscript_BeforeTranscriptReady_Throws()
    {
        var turn = InterviewTurn.CreateLearnerTurn("session-1", "My answer", 2);

        Assert.Throws<InvalidOperationException>(() =>
            turn.ConfirmTranscript("edited", learnerEdited: true));
    }

    [Fact]
    public void MarkLearnerAudioUploaded_OnInterviewerTurn_Throws()
    {
        var turn = InterviewTurn.CreateInterviewerTurn(
            "session-1", "Question?", 1,
            InterviewTurnType.OpeningQuestion,
            InterviewCapability.SelfIntroduction,
            null, null, InterviewVerificationStatus.Verified);

        Assert.Throws<InvalidOperationException>(() =>
            turn.MarkLearnerAudioUploaded("key", 3000));
    }

    [Fact]
    public void GetEvaluableTranscript_FallsBackCorrectly()
    {
        var turn = InterviewTurn.CreateLearnerTurn("session-1", "Message text", 2);

        // No transcript → use Message
        Assert.Equal("Message text", turn.GetEvaluableTranscript());

        // Raw transcript → use that
        turn.MarkLearnerAudioUploaded("key", 3000);
        turn.SetTranscript("Raw transcript text", 0.7);
        Assert.Equal("Raw transcript text", turn.GetEvaluableTranscript());

        // Confirmed → use that
        turn.ConfirmTranscript("Confirmed text", learnerEdited: true);
        Assert.Equal("Confirmed text", turn.GetEvaluableTranscript());
    }

    [Fact]
    public void Supersede_ChangesState()
    {
        var turn = InterviewTurn.CreateLearnerTurn("session-1", "First attempt", 2);
        turn.Supersede();
        Assert.Equal(InterviewTurnState.Superseded, turn.TurnState);
    }
}
