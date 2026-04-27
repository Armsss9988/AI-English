using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.InterviewPractice;

namespace EnglishCoach.Application.InterviewPractice;

/// <summary>T03: Generate an adaptive interviewer turn based on full context.</summary>
public sealed class GenerateInterviewerTurnUseCase
{
    private readonly IInterviewSessionRepository _sessionRepository;
    private readonly IInterviewProfileRepository _profileRepository;
    private readonly IAdaptiveInterviewerService _adaptiveService;
    private readonly ITextToSpeechService _ttsService;
    private readonly IInterviewAudioStorage _audioStorage;

    public GenerateInterviewerTurnUseCase(
        IInterviewSessionRepository sessionRepository,
        IInterviewProfileRepository profileRepository,
        IAdaptiveInterviewerService adaptiveService,
        ITextToSpeechService ttsService,
        IInterviewAudioStorage audioStorage)
    {
        _sessionRepository = sessionRepository;
        _profileRepository = profileRepository;
        _adaptiveService = adaptiveService;
        _ttsService = ttsService;
        _audioStorage = audioStorage;
    }

    public async Task<Contracts.InterviewPractice.AnswerQuestionResponse> ExecuteAsync(
        string sessionId, string learnerId, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.LearnerId != learnerId)
            throw new UnauthorizedAccessException("Not your session.");

        var profile = await _profileRepository.GetByIdAsync(session.InterviewProfileId, ct);
        var lastLearnerTurn = session.Turns.LastOrDefault(t => t.Role == InterviewTurnRole.Learner);

        var context = new InterviewTurnGenerationContext
        {
            SessionId = session.Id,
            InterviewMode = session.Mode,
            CvAnalysis = profile?.CvAnalysis ?? string.Empty,
            JdAnalysis = session.JdAnalysis,
            InterviewPlan = session.InterviewPlan,
            CurrentQuestionNumber = session.LearnerAnswerCount + 1,
            PlannedQuestionCount = session.PlannedQuestionCount,
            LatestLearnerTranscript = lastLearnerTurn?.GetEvaluableTranscript(),
            LatestScorecardJson = lastLearnerTurn?.ScorecardJson,
            LatestPronunciationReportJson = lastLearnerTurn?.PronunciationReportJson,
            PreviousTurnDecision = session.Turns.LastOrDefault(t =>
                t.Role == InterviewTurnRole.Interviewer)?.GetDecision(),
            ConversationHistory = session.Turns.Select(t => new InterviewTurnRecord
            {
                Speaker = t.Role == InterviewTurnRole.Interviewer ? "Interviewer" : "Learner",
                Message = t.GetEvaluableTranscript(),
                QuestionCategory = t.QuestionCategory?.ToString(),
                Timestamp = t.CreatedAtUtc
            }).ToList()
        };

        var result = await _adaptiveService.GenerateInterviewerTurnAsync(context, ct);

        if (!result.IsSuccess)
        {
            // Fallback: add a generic follow-up
            var fallbackTurn = session.AddAdaptiveInterviewerTurn(
                "Could you tell me more about your relevant experience?",
                InterviewTurnType.FollowUp,
                InterviewCapability.EnglishClarity,
                null, null, InterviewVerificationStatus.Fallback);

            await _sessionRepository.UpdateAsync(session, ct);
            return MapResponse(fallbackTurn, session, isComplete: false);
        }

        var decision = new InterviewTurnDecision
        {
            TurnType = result.TurnType,
            TargetCapability = result.TargetCapability,
            ReasonCode = result.ReasonCode,
            ShouldAdvancePlan = result.ShouldAdvancePlan,
            LearnerFacingHint = result.LearnerFacingHint
        };

        var verification = result.UsedFallback
            ? InterviewVerificationStatus.Fallback
            : InterviewVerificationStatus.Verified;

        var turn = session.AddAdaptiveInterviewerTurn(
            result.Question, result.TurnType, result.TargetCapability,
            result.Rubric, decision, verification);

        // Generate TTS audio for the interviewer turn
        await TryGenerateTtsAsync(turn, result.Question, ct);

        var isComplete = result.IsLastQuestion || session.IsQuestionLimitReached;
        if (isComplete)
            session.RequestFeedback();

        await _sessionRepository.UpdateAsync(session, ct);
        return MapResponse(turn, session, isComplete);
    }

    private async Task TryGenerateTtsAsync(InterviewTurn turn, string text, CancellationToken ct)
    {
        try
        {
            var ttsResult = await _ttsService.SynthesizeAsync(
                new TextToSpeechRequest { Text = text }, ct);

            if (ttsResult.IsSuccess && ttsResult.AudioData is not null)
            {
                var storageResult = await _audioStorage.SaveAsync(new AudioStorageRequest
                {
                    SessionId = turn.SessionId,
                    TurnId = turn.Id,
                    Purpose = "interviewer",
                    AudioData = ttsResult.AudioData,
                    ContentType = ttsResult.ContentType
                }, ct);

                if (storageResult.IsSuccess)
                    turn.MarkAudioReady(storageResult.StorageKey, ttsResult.DurationMs);
            }
        }
        catch
        {
            // TTS failure does not block text flow
        }
    }

    private static Contracts.InterviewPractice.AnswerQuestionResponse MapResponse(
        InterviewTurn turn, InterviewSession session, bool isComplete)
    {
        return new Contracts.InterviewPractice.AnswerQuestionResponse(
            turn.Message,
            turn.QuestionCategory?.ToString(),
            turn.TurnType?.ToString(),
            turn.TargetCapability?.ToString(),
            turn.GetDecision()?.LearnerFacingHint,
            isComplete,
            session.LearnerAnswerCount,
            session.PlannedQuestionCount,
            turn.AudioStorageKey.Length > 0 ? $"/me/interview/turns/{turn.Id}/audio" : null
        );
    }
}
