using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.InterviewPractice;

namespace EnglishCoach.Application.InterviewPractice;

public sealed class FinalizeInterviewUseCase
{
    private readonly IInterviewSessionRepository _sessionRepository;
    private readonly IInterviewProfileRepository _profileRepository;
    private readonly IInterviewConductorService _conductorService;

    public FinalizeInterviewUseCase(
        IInterviewSessionRepository sessionRepository,
        IInterviewProfileRepository profileRepository,
        IInterviewConductorService conductorService)
    {
        _sessionRepository = sessionRepository;
        _profileRepository = profileRepository;
        _conductorService = conductorService;
    }

    public async Task<EnglishCoach.Contracts.InterviewPractice.InterviewFeedbackResponse> ExecuteAsync(
        string learnerId,
        Guid sessionId,
        CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId.ToString(), ct);
        if (session is null)
            throw new InvalidOperationException("Interview session not found.");
        if (session.LearnerId != learnerId)
            throw new InvalidOperationException("Session does not belong to this learner.");

        // Transition to AwaitingFeedback if still Active
        if (session.State == InterviewSessionState.Active)
        {
            session.RequestFeedback();
        }

        if (session.State != InterviewSessionState.AwaitingFeedback)
            throw new InvalidOperationException($"Cannot finalize from state {session.State}.");

        // Load profile for CV context
        var profile = await _profileRepository.GetByIdAsync(session.InterviewProfileId, ct);

        // Build context for evaluation
        var conversationHistory = session.Turns.Select(t => new InterviewTurnRecord
        {
            Speaker = t.Role == InterviewTurnRole.Interviewer ? "Interviewer" : "Learner",
            Message = t.Message,
            QuestionCategory = t.QuestionCategory?.ToString(),
            Timestamp = t.CreatedAtUtc
        }).ToList();

        var context = new InterviewConductorContext
        {
            SessionId = session.Id,
            CvAnalysis = profile?.CvAnalysis ?? string.Empty,
            JdAnalysis = session.JdAnalysis,
            InterviewPlan = session.InterviewPlan,
            InterviewType = session.Type,
            ConversationHistory = conversationHistory,
            PlannedQuestionCount = session.PlannedQuestionCount,
            CurrentQuestionNumber = session.LearnerAnswerCount
        };

        // Evaluate entire session
        var feedbackResult = await _conductorService.EvaluateSessionAsync(context, ct);
        if (!feedbackResult.IsSuccess || feedbackResult.Content is null)
            throw new InvalidOperationException("Failed to evaluate session: " + feedbackResult.ErrorMessage);

        var content = feedbackResult.Content;

        var feedback = new InterviewFeedback(
            content.OverallScore,
            content.CommunicationScore,
            content.TechnicalAccuracyScore,
            content.ConfidenceScore,
            content.DetailedFeedbackEn,
            content.DetailedFeedbackVi,
            content.StrengthAreas,
            content.ImprovementAreas,
            content.SuggestedPhrases,
            content.RetryRecommendation
        );

        session.SetFeedback(feedback);
        await _sessionRepository.UpdateAsync(session, ct);

        return new EnglishCoach.Contracts.InterviewPractice.InterviewFeedbackResponse(
            Guid.Parse(session.Id),
            content.OverallScore,
            content.CommunicationScore,
            content.TechnicalAccuracyScore,
            content.ConfidenceScore,
            content.DetailedFeedbackEn,
            content.DetailedFeedbackVi,
            content.StrengthAreas.ToList(),
            content.ImprovementAreas.ToList(),
            content.SuggestedPhrases.ToList(),
            content.RetryRecommendation
        );
    }
}
