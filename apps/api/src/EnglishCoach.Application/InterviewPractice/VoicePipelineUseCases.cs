using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.InterviewPractice;

namespace EnglishCoach.Application.InterviewPractice;

/// <summary>T05: Upload learner audio answer and start transcription.</summary>
public sealed class UploadLearnerAnswerAudioUseCase
{
    private readonly IInterviewSessionRepository _sessionRepository;
    private readonly IInterviewAudioStorage _audioStorage;
    private readonly ISpeechToTextService _sttService;

    public UploadLearnerAnswerAudioUseCase(
        IInterviewSessionRepository sessionRepository,
        IInterviewAudioStorage audioStorage,
        ISpeechToTextService sttService)
    {
        _sessionRepository = sessionRepository;
        _audioStorage = audioStorage;
        _sttService = sttService;
    }

    public async Task<Contracts.InterviewPractice.TranscriptResponse> ExecuteAsync(
        string sessionId, string learnerId, byte[] audioData, string contentType,
        int clientDurationMs, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.LearnerId != learnerId)
            throw new UnauthorizedAccessException("Not your session.");

        if (session.State != InterviewSessionState.Active)
            throw new InvalidOperationException($"Cannot upload audio in state {session.State}.");

        // Create learner turn
        session.AddLearnerTurn(string.Empty, string.Empty);
        var turn = session.Turns.Last();

        // Save audio
        var storageResult = await _audioStorage.SaveAsync(new AudioStorageRequest
        {
            SessionId = sessionId,
            TurnId = turn.Id,
            Purpose = "learner",
            AudioData = audioData,
            ContentType = contentType
        }, ct);

        if (!storageResult.IsSuccess)
            throw new InvalidOperationException($"Audio storage failed: {storageResult.ErrorMessage}");

        turn.MarkLearnerAudioUploaded(storageResult.StorageKey, clientDurationMs);

        // Auto-transcribe
        var sttResult = await _sttService.TranscribeAsync(new SpeechToTextRequest
        {
            AudioStorageKey = storageResult.StorageKey,
            ContentType = contentType
        }, ct);

        if (sttResult.IsSuccess)
        {
            turn.SetTranscript(sttResult.Transcript, sttResult.Confidence);
        }

        // Turn is already added to session.Turns, we just update it.


        await _sessionRepository.UpdateAsync(session, ct);

        return new Contracts.InterviewPractice.TranscriptResponse(
            turn.Id,
            sttResult.IsSuccess ? sttResult.Transcript : string.Empty,
            sttResult.IsSuccess ? sttResult.Confidence : 0,
            turn.TurnState.ToString()
        );
    }
}

/// <summary>T06: Confirm or edit learner transcript before evaluation.</summary>
public sealed class ConfirmLearnerTranscriptUseCase
{
    private readonly IInterviewSessionRepository _sessionRepository;

    public ConfirmLearnerTranscriptUseCase(IInterviewSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task ExecuteAsync(
        string sessionId, string learnerId, string turnId,
        string confirmedTranscript, bool learnerEdited,
        CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.LearnerId != learnerId)
            throw new UnauthorizedAccessException("Not your session.");

        var turn = session.Turns.FirstOrDefault(t => t.Id == turnId)
            ?? throw new InvalidOperationException("Turn not found.");

        turn.ConfirmTranscript(confirmedTranscript, learnerEdited);

        await _sessionRepository.UpdateAsync(session, ct);
    }
}

/// <summary>T08: Evaluate a confirmed learner answer with scorecard.</summary>
public sealed class EvaluateLearnerAnswerUseCase
{
    private readonly IInterviewSessionRepository _sessionRepository;
    private readonly IInterviewProfileRepository _profileRepository;
    private readonly IAdaptiveInterviewerService _adaptiveService;
    private readonly IPronunciationAssessmentService _pronunciationService;

    public EvaluateLearnerAnswerUseCase(
        IInterviewSessionRepository sessionRepository,
        IInterviewProfileRepository profileRepository,
        IAdaptiveInterviewerService adaptiveService,
        IPronunciationAssessmentService pronunciationService)
    {
        _sessionRepository = sessionRepository;
        _profileRepository = profileRepository;
        _adaptiveService = adaptiveService;
        _pronunciationService = pronunciationService;
    }

    public async Task<AnswerEvaluationResult> ExecuteAsync(
        string sessionId, string learnerId, string turnId,
        CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.LearnerId != learnerId)
            throw new UnauthorizedAccessException("Not your session.");

        var learnerTurn = session.Turns.FirstOrDefault(t => t.Id == turnId && t.Role == InterviewTurnRole.Learner)
            ?? throw new InvalidOperationException("Learner turn not found.");

        var confirmedTranscript = learnerTurn.GetEvaluableTranscript();
        if (string.IsNullOrWhiteSpace(confirmedTranscript))
            throw new InvalidOperationException("No transcript available for evaluation.");

        // Find the preceding interviewer turn
        var interviewerTurn = session.Turns
            .Where(t => t.Role == InterviewTurnRole.Interviewer && t.TurnOrder < learnerTurn.TurnOrder)
            .OrderByDescending(t => t.TurnOrder)
            .FirstOrDefault();

        var profile = await _profileRepository.GetByIdAsync(session.InterviewProfileId, ct);

        // Run pronunciation assessment if audio exists
        if (!string.IsNullOrWhiteSpace(learnerTurn.AudioStorageKey))
        {
            try
            {
                var pronResult = await _pronunciationService.AssessAsync(new PronunciationAssessmentRequest
                {
                    AudioStorageKey = learnerTurn.AudioStorageKey,
                    RawTranscript = learnerTurn.RawTranscript,
                    ConfirmedTranscript = confirmedTranscript,
                    QuestionText = interviewerTurn?.Message ?? string.Empty
                }, ct);

                if (pronResult.IsSuccess)
                {
                    var pronJson = System.Text.Json.JsonSerializer.Serialize(pronResult);
                    var pronStatus = pronResult.UsedFallback
                        ? InterviewVerificationStatus.Fallback
                        : InterviewVerificationStatus.Verified;
                    learnerTurn.SetPronunciationReport(pronJson, pronStatus);
                }
            }
            catch { /* Pronunciation failure does not block answer evaluation */ }
        }

        // Evaluate the answer
        var evalContext = new AnswerEvaluationContext
        {
            SessionId = session.Id,
            InterviewMode = session.Mode,
            CvAnalysis = profile?.CvAnalysis ?? string.Empty,
            JdAnalysis = session.JdAnalysis,
            QuestionText = interviewerTurn?.Message ?? string.Empty,
            ConfirmedTranscript = confirmedTranscript,
            Rubric = interviewerTurn?.GetRubric(),
            PronunciationReportJson = learnerTurn.PronunciationReportJson,
            TargetCapability = interviewerTurn?.TargetCapability ?? InterviewCapability.EnglishClarity
        };

        var evalResult = await _adaptiveService.EvaluateAnswerAsync(evalContext, ct);

        if (evalResult.IsSuccess && evalResult.Scorecard is not null)
        {
            var scorecardJson = System.Text.Json.JsonSerializer.Serialize(evalResult.Scorecard);
            var evalStatus = evalResult.UsedFallback
                ? InterviewVerificationStatus.Fallback
                : InterviewVerificationStatus.Verified;
            learnerTurn.SetScorecard(scorecardJson, evalStatus);
        }

        await _sessionRepository.UpdateAsync(session, ct);
        return evalResult;
    }
}
