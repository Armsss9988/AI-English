using System.Text.Json;
using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.InterviewPractice;

namespace EnglishCoach.Application.InterviewPractice;

public sealed class StartInterviewSessionUseCase
{
    private const int FallbackQuestionCount = 5;

    private readonly IInterviewProfileRepository _profileRepository;
    private readonly IInterviewSessionRepository _sessionRepository;
    private readonly IInterviewAnalysisService _analysisService;
    private readonly IInterviewConductorService _conductorService;

    public StartInterviewSessionUseCase(
        IInterviewProfileRepository profileRepository,
        IInterviewSessionRepository sessionRepository,
        IInterviewAnalysisService analysisService,
        IInterviewConductorService conductorService)
    {
        _profileRepository = profileRepository;
        _sessionRepository = sessionRepository;
        _analysisService = analysisService;
        _conductorService = conductorService;
    }

    public async Task<EnglishCoach.Contracts.InterviewPractice.StartInterviewResponse> ExecuteAsync(
        string learnerId,
        EnglishCoach.Contracts.InterviewPractice.StartInterviewRequest request,
        CancellationToken ct = default)
    {
        // Load interview profile (CV analysis)
        var profile = await _profileRepository.GetByIdAsync(request.ProfileId.ToString(), ct);
        if (profile is null || string.IsNullOrEmpty(profile.CvAnalysis))
            throw new InvalidOperationException("Interview profile not found or CV not analyzed.");

        if (profile.LearnerId != learnerId)
            throw new InvalidOperationException("Profile does not belong to this learner.");

        // Parse interview type
        if (!Enum.TryParse<InterviewType>(request.InterviewType, true, out var interviewType))
            interviewType = InterviewType.Mixed;

        // Create session
        var sessionId = Guid.NewGuid().ToString();
        var session = InterviewSession.Create(sessionId, learnerId, profile.Id, request.JdText, interviewType);

        // Start analysis
        session.StartAnalysis();

        var jdResult = await _analysisService.AnalyzeJdAsync(request.JdText, profile.CvAnalysis, ct);
        var usedAnalysisFallback = !jdResult.IsSuccess || string.IsNullOrWhiteSpace(jdResult.Analysis);
        var jdAnalysis = usedAnalysisFallback
            ? CreateFallbackJdAnalysis(request.JdText)
            : jdResult.Analysis!;

        InterviewPlanResult? planResult = null;
        if (!usedAnalysisFallback)
        {
            planResult = await _analysisService.CreateInterviewPlanAsync(
                profile.CvAnalysis, jdAnalysis, interviewType, ct);
        }

        var usedPlanFallback = planResult is null || !planResult.IsSuccess || string.IsNullOrWhiteSpace(planResult.Plan);
        var interviewPlan = usedPlanFallback
            ? CreateFallbackInterviewPlan(interviewType)
            : planResult!.Plan!;
        var plannedQuestionCount = usedPlanFallback
            ? FallbackQuestionCount
            : Math.Max(1, planResult!.RecommendedQuestionCount);

        session.CompleteAnalysis(jdAnalysis, interviewPlan, plannedQuestionCount);

        // Generate first question
        var context = new InterviewConductorContext
        {
            SessionId = sessionId,
            CvAnalysis = profile.CvAnalysis,
            JdAnalysis = jdAnalysis,
            InterviewPlan = interviewPlan,
            InterviewType = interviewType,
            PlannedQuestionCount = plannedQuestionCount,
            CurrentQuestionNumber = 1
        };

        var firstQuestion = CreateFallbackFirstQuestion(interviewType);
        if (!usedAnalysisFallback && !usedPlanFallback)
        {
            var questionResult = await _conductorService.GenerateNextQuestionAsync(context, ct);
            firstQuestion = questionResult.IsSuccess && questionResult.Content is not null
                ? questionResult.Content
                : firstQuestion;
        }

        // Parse category
        if (!Enum.TryParse<InterviewQuestionCategory>(firstQuestion.Category, true, out var category))
            category = InterviewQuestionCategory.Opening;

        session.AddInterviewerTurn(firstQuestion.Question, category);

        await _sessionRepository.CreateAsync(session, ct);

        return new EnglishCoach.Contracts.InterviewPractice.StartInterviewResponse(
            Guid.Parse(sessionId),
            session.State.ToString(),
            interviewType.ToString(),
            session.Mode.ToString(),
            plannedQuestionCount,
            firstQuestion.Question,
            firstQuestion.Category,
            null,
            null,
            firstQuestion.CoachingHint,
            null
        );
    }

    private static InterviewQuestionContent CreateFallbackFirstQuestion(InterviewType interviewType)
    {
        return new InterviewQuestionContent
        {
            Question = interviewType == InterviewType.Technical
                ? "Could you walk me through your technical background and one backend project you are proud of?"
                : "Could you walk me through your background and the experience most relevant to this role?",
            Category = "Opening",
            CoachingHint = "Start with your current role, years of experience, strongest skills, and one concrete project example.",
            IsLastQuestion = false
        };
    }

    private static string CreateFallbackJdAnalysis(string jdText)
    {
        return JsonSerializer.Serialize(new
        {
            companyType = "Unknown",
            roleLevel = "Unknown",
            requiredSkills = Array.Empty<string>(),
            niceToHave = Array.Empty<string>(),
            keyResponsibilities = Array.Empty<string>(),
            communicationRequirements = "General English interview communication",
            skillGaps = Array.Empty<string>(),
            matchScore = 0,
            source = "fallback",
            jdExcerpt = Truncate(jdText, 500)
        });
    }

    private static string CreateFallbackInterviewPlan(InterviewType interviewType)
    {
        return JsonSerializer.Serialize(new
        {
            interviewType = interviewType.ToString(),
            totalQuestions = FallbackQuestionCount,
            focusAreas = new[] { "background", "role fit", "communication clarity" },
            questionBreakdown = new
            {
                behavioral = interviewType == InterviewType.Technical ? 1 : 2,
                technical = interviewType == InterviewType.Behavioral ? 1 : 2,
                situational = 1
            },
            specialFocus = "Fallback plan used because the AI provider was unavailable."
        });
    }

    private static string Truncate(string value, int maxLength)
    {
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
