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
  ""keyResponsibilities"": [""Design and build RESTful APIs"", ""Collaborate with cross-functional teams"", ""Participate in code reviews""],
  ""communicationRequirements"": ""Daily standups with English-speaking team, client demos"",
  ""skillGaps"": [""Microservices architecture"", ""Kubernetes""],
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
            _ => 7 // Mixed
        };

        var plan = $@"{{
  ""interviewType"": ""{interviewType}"",
  ""totalQuestions"": {questionCount},
  ""focusAreas"": [""Technical skills"", ""Communication"", ""Problem-solving"", ""Teamwork""],
  ""questionBreakdown"": {{
    ""behavioral"": 2,
    ""technical"": 3,
    ""situational"": 2
  }},
  ""specialFocus"": ""English communication in client-facing scenarios""
}}";
        return Task.FromResult(InterviewPlanResult.Success(plan, questionCount, ProviderKind.Fake));
    }
}

public class FakeInterviewConductorService : IInterviewConductorService
{
    private static readonly string[] FakeQuestions = new[]
    {
        "Tell me about yourself and your experience as a developer.",
        "Can you describe a challenging project you worked on recently? What was your role and how did you handle it?",
        "How do you typically communicate progress updates to English-speaking clients?",
        "Imagine you discover a critical bug right before a release deadline. Walk me through how you would handle this situation.",
        "Can you explain the difference between a monolithic architecture and microservices? When would you choose one over the other?",
        "Tell me about a time when you had a disagreement with a team member about a technical approach. How did you resolve it?",
        "How would you explain a complex technical issue to a non-technical stakeholder in English?",
        "What strategies do you use for debugging production issues? Can you walk me through your process?"
    };

    private static readonly string[] Categories = new[]
    {
        "Opening", "Behavioral", "Behavioral", "Situational", "Technical", "Behavioral", "Situational", "Technical"
    };

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
            CoachingHint = isLast ? null : "Try to use specific examples and professional vocabulary.",
            IsLastQuestion = isLast
        };

        return Task.FromResult(InterviewQuestionResult.Success(content, ProviderKind.Fake));
    }

    public Task<InterviewFeedbackResult> EvaluateSessionAsync(
        InterviewConductorContext context, CancellationToken ct = default)
    {
        var content = new InterviewFeedbackContent
        {
            OverallScore = 72,
            CommunicationScore = 68,
            TechnicalAccuracyScore = 78,
            ConfidenceScore = 65,
            DetailedFeedbackEn = "Good technical knowledge demonstrated. Your answers show solid understanding of the technologies mentioned in the JD. However, there's room for improvement in structuring your responses using the STAR method. Your English communication is understandable but could benefit from more professional vocabulary and smoother transitions between ideas.",
            DetailedFeedbackVi = "Kiến thức kỹ thuật tốt. Câu trả lời của bạn thể hiện hiểu biết vững về các công nghệ trong JD. Tuy nhiên, cần cải thiện cách trình bày câu trả lời theo phương pháp STAR. Giao tiếp tiếng Anh có thể hiểu được nhưng cần thêm từ vựng chuyên nghiệp và chuyển ý mượt mà hơn.",
            StrengthAreas = new[] { "Technical depth", "Problem-solving approach", "Honest about gaps" },
            ImprovementAreas = new[] { "Use STAR method for behavioral questions", "Reduce filler words (um, uh)", "Practice professional transitions", "Expand vocabulary for system design discussions" },
            SuggestedPhrases = new[] { "In my experience...", "Let me walk you through...", "The approach I would take is...", "To elaborate on that point...", "From a technical perspective..." },
            RetryRecommendation = "Focus on practicing behavioral questions using the STAR method. Record yourself answering and listen for filler words."
        };

        return Task.FromResult(InterviewFeedbackResult.Success(content, ProviderKind.Fake));
    }
}
