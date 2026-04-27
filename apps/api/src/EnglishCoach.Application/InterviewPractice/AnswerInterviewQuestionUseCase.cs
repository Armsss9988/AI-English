using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.InterviewPractice;

namespace EnglishCoach.Application.InterviewPractice;

public sealed class AnswerInterviewQuestionUseCase
{
    private readonly IInterviewSessionRepository _sessionRepository;
    private readonly IInterviewProfileRepository _profileRepository;
    private readonly IInterviewConductorService _conductorService;

    public AnswerInterviewQuestionUseCase(
        IInterviewSessionRepository sessionRepository,
        IInterviewProfileRepository profileRepository,
        IInterviewConductorService conductorService)
    {
        _sessionRepository = sessionRepository;
        _profileRepository = profileRepository;
        _conductorService = conductorService;
    }

    public async Task<EnglishCoach.Contracts.InterviewPractice.AnswerQuestionResponse> ExecuteAsync(
        string learnerId,
        Guid sessionId,
        EnglishCoach.Contracts.InterviewPractice.AnswerQuestionRequest request,
        CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId.ToString(), ct);
        if (session is null)
            throw new InvalidOperationException("Interview session not found.");
        if (session.LearnerId != learnerId)
            throw new InvalidOperationException("Session does not belong to this learner.");

        // Add learner's answer
        session.AddLearnerTurn(request.Answer, request.AudioUrl ?? string.Empty);

        // Check if we've reached the question limit
        if (session.IsQuestionLimitReached)
        {
            session.RequestFeedback();
            await _sessionRepository.UpdateAsync(session, ct);

            return new EnglishCoach.Contracts.InterviewPractice.AnswerQuestionResponse(
                null,
                null,
                null,
                null,
                null,
                true,
                session.LearnerAnswerCount,
                session.PlannedQuestionCount,
                null
            );
        }

        // Load profile for CV analysis context
        var profile = await _profileRepository.GetByIdAsync(session.InterviewProfileId, ct);

        // Build conductor context
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
            LatestLearnerAnswer = conversationHistory.LastOrDefault(t => t.Speaker == "Learner"),
            PlannedQuestionCount = session.PlannedQuestionCount,
            CurrentQuestionNumber = session.LearnerAnswerCount + 1
        };

        var questionResult = await _conductorService.GenerateNextQuestionAsync(context, ct);
        var nextQuestion = questionResult.IsSuccess && questionResult.Content is not null
            ? questionResult.Content
            : CreateFallbackNextQuestion(session.Type, session.LearnerAnswerCount, session.PlannedQuestionCount);

        // Parse category
        if (!Enum.TryParse<InterviewQuestionCategory>(nextQuestion.Category, true, out var category))
            category = InterviewQuestionCategory.FollowUp;

        session.AddInterviewerTurn(nextQuestion.Question, category);

        await _sessionRepository.UpdateAsync(session, ct);

        return new EnglishCoach.Contracts.InterviewPractice.AnswerQuestionResponse(
            nextQuestion.Question,
            nextQuestion.Category,
            null,
            null,
            nextQuestion.CoachingHint,
            nextQuestion.IsLastQuestion,
            session.LearnerAnswerCount,
            session.PlannedQuestionCount,
            null
        );
    }

    private static InterviewQuestionContent CreateFallbackNextQuestion(
        InterviewType interviewType,
        int answeredCount,
        int plannedQuestionCount)
    {
        var isLastQuestion = answeredCount + 1 >= plannedQuestionCount;
        var focus = interviewType == InterviewType.Technical
            ? "technical decision"
            : "experience";

        return new InterviewQuestionContent
        {
            Question = isLastQuestion
                ? "Thanks. To close, could you summarize why you are a strong fit for this role?"
                : $"Thanks for sharing that. Could you give one specific example from your {focus}, including your role, actions, and result?",
            Category = isLastQuestion ? "Closing" : "FollowUp",
            CoachingHint = "Use a concise STAR structure: situation, task, action, result.",
            IsLastQuestion = isLastQuestion
        };
    }
}
