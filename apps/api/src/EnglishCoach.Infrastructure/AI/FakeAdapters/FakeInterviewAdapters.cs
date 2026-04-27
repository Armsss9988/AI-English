using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.InterviewPractice;

namespace EnglishCoach.Infrastructure.AI.FakeAdapters;

public class FakeInterviewAnalysisService : IInterviewAnalysisService
{
    public ProviderKind Provider => ProviderKind.Fake;

    public Task<CvAnalysisResult> AnalyzeCvAsync(string cvText, CancellationToken ct = default)
    {
        var analysis = @"{
  ""name"": ""Nguyen Van A"",
  ""yearsOfExperience"": 3,
  ""skills"": [""C#"", ""ASP.NET Core"", ""React"", ""TypeScript"", ""PostgreSQL"", ""Docker""],
  ""strengths"": [""Backend API development"", ""Database design"", ""REST API patterns""],
  ""weaknesses"": [""Limited experience with system design"", ""English communication needs improvement""],
  ""currentRole"": ""Full-stack Developer"",
  ""education"": ""Bachelor of Computer Science""
}";
        return Task.FromResult(CvAnalysisResult.Success(analysis, ProviderKind.Fake));
    }

    public Task<JdAnalysisResult> AnalyzeJdAsync(string jdText, string cvAnalysis, CancellationToken ct = default)
    {
        var analysis = @"{
  ""companyType"": ""International SaaS startup"",
  ""roleLevel"": ""Mid-Senior"",
  ""requiredSkills"": [""C#"", ""ASP.NET Core"", ""React"", ""PostgreSQL"", ""Microservices""],
  ""niceToHave"": [""Kubernetes"", ""Azure"", ""CI/CD pipelines""],
  ""keyResponsibilities"": [""Design and build RESTful APIs"", ""Collaborate with cross-functional teams""],
  ""communicationRequirements"": ""Daily standups with English-speaking team, client demos"",
  ""matchScore"": 75
}";
        return Task.FromResult(JdAnalysisResult.Success(analysis, ProviderKind.Fake));
    }

    public Task<InterviewPlanResult> CreateInterviewPlanAsync(
        string cvAnalysis, string jdAnalysis, InterviewType interviewType, CancellationToken ct = default)
    {
        var questionCount = interviewType switch
        {
            InterviewType.Behavioral => 6,
            InterviewType.Technical => 8,
            InterviewType.Situational => 6,
            _ => 7
        };
        var plan = $@"{{""interviewType"":""{interviewType}"",""totalQuestions"":{questionCount}}}";
        return Task.FromResult(InterviewPlanResult.Success(plan, questionCount, ProviderKind.Fake));
    }
}

public class FakeInterviewConductorService : IInterviewConductorService
{
    private static readonly string[] FakeQuestions = new[]
    {
        "Tell me about yourself and your experience as a developer.",
        "Can you describe a challenging project you worked on recently?",
        "How do you communicate progress to English-speaking clients?",
        "Walk me through how you would handle a critical bug before release.",
        "Explain the difference between monolithic and microservices architecture.",
        "Tell me about a disagreement with a team member about a technical approach.",
        "How would you explain a complex technical issue to a non-technical stakeholder?",
        "What strategies do you use for debugging production issues?"
    };

    private static readonly string[] Categories = new[]
    { "Opening", "Behavioral", "Behavioral", "Situational", "Technical", "Behavioral", "Situational", "Technical" };

    public ProviderKind Provider => ProviderKind.Fake;

    public Task<InterviewQuestionResult> GenerateNextQuestionAsync(
        InterviewConductorContext context, CancellationToken ct = default)
    {
        var questionIndex = Math.Min(context.CurrentQuestionNumber - 1, FakeQuestions.Length - 1);
        var isLast = context.CurrentQuestionNumber >= context.PlannedQuestionCount;
        var content = new InterviewQuestionContent
        {
            Question = FakeQuestions[questionIndex],
            Category = Categories[questionIndex],
            CoachingHint = isLast ? null : "Try to use specific examples.",
            IsLastQuestion = isLast
        };
        return Task.FromResult(InterviewQuestionResult.Success(content, ProviderKind.Fake));
    }

    public Task<InterviewFeedbackResult> EvaluateSessionAsync(
        InterviewConductorContext context, CancellationToken ct = default)
    {
        var content = new InterviewFeedbackContent
        {
            OverallScore = 72, CommunicationScore = 68,
            TechnicalAccuracyScore = 78, ConfidenceScore = 65,
            DetailedFeedbackEn = "Good technical knowledge. Room for improvement in STAR method and professional vocabulary.",
            DetailedFeedbackVi = "Kiến thức kỹ thuật tốt. Cần cải thiện phương pháp STAR và từ vựng chuyên nghiệp.",
            StrengthAreas = new[] { "Technical depth", "Problem-solving", "Honest about gaps" },
            ImprovementAreas = new[] { "Use STAR method", "Reduce filler words", "Professional transitions" },
            SuggestedPhrases = new[] { "In my experience...", "Let me walk you through...", "The approach I would take is..." },
            RetryRecommendation = "Practice behavioral questions using the STAR method."
        };
        return Task.FromResult(InterviewFeedbackResult.Success(content, ProviderKind.Fake));
    }
}

// ---- T02: Adaptive Interviewer (context-aware, not static) ----

public class FakeAdaptiveInterviewerService : IAdaptiveInterviewerService
{
    private static readonly InterviewCapability[] CapabilityRotation = new[]
    {
        InterviewCapability.SelfIntroduction,
        InterviewCapability.ProjectDeepDive,
        InterviewCapability.TechnicalTradeoff,
        InterviewCapability.BehavioralStar,
        InterviewCapability.ClientCommunication,
        InterviewCapability.RequirementClarification,
        InterviewCapability.IncidentConflictStory,
        InterviewCapability.EnglishClarity,
        InterviewCapability.PronunciationClarity,
        InterviewCapability.WeakSpotRetry
    };

    public ProviderKind Provider => ProviderKind.Fake;

    public Task<InterviewTurnGenerationResult> GenerateInterviewerTurnAsync(
        InterviewTurnGenerationContext context, CancellationToken ct = default)
    {
        // Adaptive logic based on context (not static array)
        var turnType = InterviewTurnType.MainQuestion;
        var reasonCode = "normal_progression";
        var shouldAdvance = true;
        string? hint = context.InterviewMode == InterviewMode.TrainingInterview
            ? "Take your time and use specific examples." : null;

        // If no learner answer yet → opening
        if (string.IsNullOrWhiteSpace(context.LatestLearnerTranscript))
        {
            turnType = InterviewTurnType.OpeningQuestion;
            reasonCode = "no_learner_answer";
        }
        // If latest answer is short (< 25 words) → follow-up
        else if (context.LatestLearnerTranscript.Split(' ').Length < 25)
        {
            turnType = InterviewTurnType.FollowUp;
            reasonCode = "shallow_answer";
            shouldAdvance = false;
            hint = context.InterviewMode == InterviewMode.TrainingInterview
                ? "Try to elaborate with more details and examples." : null;
        }
        // If pronunciation report has issues → clarification
        else if (!string.IsNullOrWhiteSpace(context.LatestPronunciationReportJson)
                 && context.LatestPronunciationReportJson.Contains("\"severity\":\"High\""))
        {
            turnType = InterviewTurnType.Clarification;
            reasonCode = "low_pronunciation_confidence";
            shouldAdvance = false;
        }

        var capabilityIndex = (context.CurrentQuestionNumber - 1) % CapabilityRotation.Length;
        var capability = context.CurrentCapabilityTarget ?? CapabilityRotation[capabilityIndex];
        var isLast = context.CurrentQuestionNumber >= context.PlannedQuestionCount;

        var question = GenerateQuestionForCapability(capability, turnType);
        var rubric = new InterviewQuestionRubric
        {
            Capability = capability,
            SuccessCriteria = $"Demonstrate {capability} with specific examples.",
            AnswerStructureHint = capability == InterviewCapability.BehavioralStar ? "Use STAR method" : "Be specific"
        };

        return Task.FromResult(InterviewTurnGenerationResult.Success(
            question, turnType, capability, rubric, shouldAdvance,
            reasonCode, hint, isLast, ProviderKind.Fake, "fake-adaptive-v1", usedFallback: true));
    }

    public Task<AnswerEvaluationResult> EvaluateAnswerAsync(
        AnswerEvaluationContext context, CancellationToken ct = default)
    {
        var wordCount = context.ConfirmedTranscript.Split(' ').Length;
        var baseScore = Math.Min(100, 40 + wordCount * 2);

        var card = new AnswerScorecardContent
        {
            ContentFitScore = baseScore,
            JdRelevanceScore = baseScore - 5,
            CvEvidenceScore = baseScore - 10,
            StructureScore = baseScore - 8,
            TechnicalCredibilityScore = baseScore,
            EnglishClarityScore = baseScore - 15,
            ProfessionalToneScore = baseScore - 5,
            PronunciationClarityScore = baseScore - 20,
            FluencyScore = baseScore - 10,
            OverallScore = baseScore - 8,
            Evidence = "Mentioned relevant technologies.",
            MissingEvidence = "Could provide more specific metrics.",
            BetterAnswer = "A stronger answer would include: specific metrics, timeline, and your unique contribution.",
            Corrections = new[]
            {
                new AnswerCorrection
                {
                    Original = "I do the backend",
                    Corrected = "I was responsible for the backend architecture",
                    ExplanationVi = "Dùng 'was responsible for' thay vì 'do' để thể hiện trách nhiệm chuyên nghiệp hơn."
                }
            },
            RetryDrillPrompt = "Try again: mention a specific project, your role, and measurable outcomes.",
            PhraseCandidates = new[] { "I was responsible for...", "The key challenge was...", "As a result, we achieved..." },
            MistakeCandidates = new[] { "Vague language: 'I do the backend'" },
            RequiresRetry = baseScore < 60
        };

        return Task.FromResult(AnswerEvaluationResult.Success(card, ProviderKind.Fake, fallback: true));
    }

    private static string GenerateQuestionForCapability(InterviewCapability cap, InterviewTurnType turnType)
    {
        if (turnType == InterviewTurnType.FollowUp)
            return "Could you give me more specific details about that? Perhaps a concrete example with measurable outcomes?";
        if (turnType == InterviewTurnType.Clarification)
            return "I didn't quite catch that. Could you repeat your answer more slowly?";

        return cap switch
        {
            InterviewCapability.SelfIntroduction => "Tell me about yourself and your experience as a developer.",
            InterviewCapability.ProjectDeepDive => "Describe a challenging project you led. What was your role and the outcome?",
            InterviewCapability.TechnicalTradeoff => "Can you explain a technical decision where you had to choose between two approaches?",
            InterviewCapability.BehavioralStar => "Tell me about a time when you faced a significant challenge at work. How did you handle it?",
            InterviewCapability.ClientCommunication => "How do you typically explain technical concepts to non-technical stakeholders?",
            InterviewCapability.RequirementClarification => "If you received vague requirements, how would you clarify them?",
            InterviewCapability.IncidentConflictStory => "Tell me about a disagreement with a team member. How was it resolved?",
            InterviewCapability.EnglishClarity => "How would you present a project status update to an English-speaking client?",
            InterviewCapability.PronunciationClarity => "Can you walk me through your daily development workflow?",
            InterviewCapability.WeakSpotRetry => "Let's revisit the previous topic. Can you provide a stronger example?",
            _ => "Tell me more about your relevant experience."
        };
    }
}

// ---- T04: Fake TTS ----

public class FakeTextToSpeechService : ITextToSpeechService
{
    public ProviderKind Provider => ProviderKind.Fake;

    public Task<TextToSpeechResult> SynthesizeAsync(TextToSpeechRequest request, CancellationToken ct = default)
    {
        // Generate a tiny silent audio stub (just enough for testing)
        var fakeAudio = new byte[1024];
        var estimatedDurationMs = request.Text.Length * 60; // ~60ms per character
        return Task.FromResult(TextToSpeechResult.Success(
            fakeAudio, "audio/mp3", estimatedDurationMs, ProviderKind.Fake));
    }
}

// ---- T06: Fake STT ----

public class FakeSpeechToTextService : ISpeechToTextService
{
    public ProviderKind Provider => ProviderKind.Fake;

    public Task<SpeechToTextResult> TranscribeAsync(SpeechToTextRequest request, CancellationToken ct = default)
    {
        // Deterministic fake transcript for testing
        var transcript = "I have three years of experience working with ASP.NET Core and React.";
        var wordTimings = transcript.Split(' ').Select((w, i) => new WordTiming
        {
            Word = w, Confidence = 0.85 + (i % 3) * 0.05,
            StartMs = i * 400, EndMs = (i + 1) * 400
        }).ToArray();

        return Task.FromResult(SpeechToTextResult.Success(
            transcript, 0.87, ProviderKind.Fake, wordTimings, usedFallback: true));
    }
}

// ---- T07: Fake Pronunciation Assessment ----

public class FakePronunciationAssessmentService : IPronunciationAssessmentService
{
    public ProviderKind Provider => ProviderKind.Fake;

    public Task<PronunciationAssessmentResult> AssessAsync(
        PronunciationAssessmentRequest request, CancellationToken ct = default)
    {
        var issues = new List<PronunciationWordIssue>();

        // Flag low-confidence STT words as pronunciation issues
        if (request.WordConfidences is { Count: > 0 })
        {
            foreach (var word in request.WordConfidences.Where(w => w.Confidence < 0.7))
            {
                issues.Add(new PronunciationWordIssue
                {
                    HeardAs = word.Word,
                    Expected = word.Word,
                    IssueType = "LowConfidenceWord",
                    ExplanationVi = $"Từ '{word.Word}' chưa được phát âm rõ ràng.",
                    CorrectPronunciationText = word.Word,
                    Severity = "Medium"
                });
            }
        }

        // Add a deterministic technical term issue for testing
        if (request.ConfirmedTranscript.Contains("ASP.NET", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add(new PronunciationWordIssue
            {
                HeardAs = "asp net",
                Expected = "ASP.NET",
                IssueType = "TechnicalTerm",
                ExplanationVi = "Phát âm 'ASP.NET' theo từng chữ cái: A-S-P dot NET.",
                CorrectPronunciationText = "A-S-P dot NET",
                Ipa = "/eɪ ɛs piː dɒt nɛt/",
                Severity = "Low"
            });
        }

        return Task.FromResult(PronunciationAssessmentResult.Success(
            overall: 72, fluency: 70, accuracy: 75, completeness: 80,
            issues, ProviderKind.Fake, fallback: true));
    }
}
