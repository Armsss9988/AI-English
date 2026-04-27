using EnglishCoach.Domain.InterviewPractice;

namespace EnglishCoach.Application.InterviewPractice;

/// <summary>T10: Get full session detail for resume/replay.</summary>
public sealed class GetInterviewSessionQuery
{
    private readonly IInterviewSessionRepository _sessionRepository;

    public GetInterviewSessionQuery(IInterviewSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Contracts.InterviewPractice.InterviewSessionDetailResponse?> ExecuteAsync(
        string sessionId, string learnerId, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct);
        if (session is null || session.LearnerId != learnerId)
            return null;

        var turns = session.Turns.OrderBy(t => t.TurnOrder).Select(t =>
            new Contracts.InterviewPractice.InterviewTurnDto(
                t.Id,
                t.Role == InterviewTurnRole.Interviewer ? "interviewer" : "learner",
                t.Message,
                t.TurnType?.ToString(),
                t.TargetCapability?.ToString(),
                t.QuestionCategory?.ToString(),
                t.AudioStorageKey.Length > 0 ? $"/me/interview/turns/{t.Id}/audio" : null,
                t.AudioDurationMs > 0 ? t.AudioDurationMs : null,
                string.IsNullOrWhiteSpace(t.RawTranscript) ? null : t.RawTranscript,
                string.IsNullOrWhiteSpace(t.ConfirmedTranscript) ? null : t.ConfirmedTranscript,
                t.TranscriptConfidence > 0 ? t.TranscriptConfidence : null,
                t.GetDecision()?.LearnerFacingHint,
                t.TurnState.ToString(),
                t.VerificationStatus.ToString(),
                t.CreatedAtUtc
            )).ToList();

        Contracts.InterviewPractice.InterviewFeedbackResponse? feedback = null;
        if (session.Feedback is not null)
        {
            var fb = session.Feedback;
            feedback = new Contracts.InterviewPractice.InterviewFeedbackResponse(
                System.Guid.Parse(session.Id),
                fb.OverallScore, fb.CommunicationScore,
                fb.TechnicalAccuracyScore, fb.ConfidenceScore,
                fb.DetailedFeedbackEn, fb.DetailedFeedbackVi,
                fb.StrengthAreas.ToList(), fb.ImprovementAreas.ToList(),
                fb.SuggestedPhrases.ToList(), fb.RetryRecommendation
            );
        }

        return new Contracts.InterviewPractice.InterviewSessionDetailResponse(
            System.Guid.Parse(session.Id),
            session.Type.ToString(),
            session.Mode.ToString(),
            session.State.ToString(),
            session.PlannedQuestionCount,
            session.LearnerAnswerCount,
            session.JdAnalysis.Length > 200 ? session.JdAnalysis[..200] + "..." : session.JdAnalysis,
            turns,
            feedback
        );
    }
}
