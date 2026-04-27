using System.ClientModel;
using System.Text.Json;
using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.InterviewPractice;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace EnglishCoach.Infrastructure.AI.OpenAI;

public class NimInterviewAnalysisService : IInterviewAnalysisService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<NimInterviewAnalysisService> _logger;

    public ProviderKind Provider => ProviderKind.OpenAI;

    public NimInterviewAnalysisService(
        IOptions<OpenAIOptions> options,
        ILogger<NimInterviewAnalysisService> logger)
    {
        _logger = logger;
        var opts = options.Value;
        var clientOptions = NimClientOptionsFactory.Create(opts);
        _chatClient = new ChatClient(opts.ChatModel, new ApiKeyCredential(opts.ApiKey), clientOptions);
    }

    public async Task<CvAnalysisResult> AnalyzeCvAsync(string cvText, CancellationToken ct = default)
    {
        try
        {
            var systemPrompt = @"You are a professional HR/recruitment analyst. Analyze this CV/resume for an IT professional.
Output ONLY valid JSON with this structure:
{
  ""name"": ""string"",
  ""yearsOfExperience"": number,
  ""skills"": [""string""],
  ""strengths"": [""string""],
  ""weaknesses"": [""string""],
  ""currentRole"": ""string"",
  ""education"": ""string"",
  ""notableProjects"": [""string""]
}";
            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage(systemPrompt),
                ChatMessage.CreateUserMessage($"CV Content:\n{cvText}")
            };

            var options = new ChatCompletionOptions { Temperature = 0.3f };
            var response = await _chatClient.CompleteChatAsync(messages, options, ct);
            var content = CleanJsonString(response.Value.Content[0].Text);

            return CvAnalysisResult.Success(content, Provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze CV.");
            return CvAnalysisResult.Failure(ex.Message, Provider);
        }
    }

    public async Task<JdAnalysisResult> AnalyzeJdAsync(string jdText, string cvAnalysis, CancellationToken ct = default)
    {
        try
        {
            var systemPrompt = @"You are a recruitment analyst. Analyze this Job Description and compare it against the candidate's CV analysis.
Output ONLY valid JSON with this structure:
{
  ""companyType"": ""string"",
  ""roleLevel"": ""string"",
  ""requiredSkills"": [""string""],
  ""niceToHave"": [""string""],
  ""keyResponsibilities"": [""string""],
  ""communicationRequirements"": ""string"",
  ""skillGaps"": [""string""],
  ""matchScore"": number (0-100)
}";
            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage(systemPrompt),
                ChatMessage.CreateUserMessage($"Job Description:\n{jdText}\n\nCandidate CV Analysis:\n{cvAnalysis}")
            };

            var options = new ChatCompletionOptions { Temperature = 0.3f };
            var response = await _chatClient.CompleteChatAsync(messages, options, ct);
            var content = CleanJsonString(response.Value.Content[0].Text);

            return JdAnalysisResult.Success(content, Provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze JD.");
            return JdAnalysisResult.Failure(ex.Message, Provider);
        }
    }

    public async Task<InterviewPlanResult> CreateInterviewPlanAsync(
        string cvAnalysis, string jdAnalysis, InterviewType interviewType, CancellationToken ct = default)
    {
        try
        {
            var systemPrompt = $@"You are an expert interview coach creating a personalized mock interview plan.
Interview type: {interviewType}

Based on the CV analysis and JD analysis, create an interview plan.
The number of questions should be based on the complexity of the JD and the candidate's experience level:
- Simple JD + experienced candidate: 5-6 questions
- Complex JD + junior candidate: 8-10 questions
- Average: 6-8 questions

Output ONLY valid JSON with this structure:
{{
  ""interviewType"": ""string"",
  ""totalQuestions"": number,
  ""focusAreas"": [""string""],
  ""questionBreakdown"": {{
    ""behavioral"": number,
    ""technical"": number,
    ""situational"": number
  }},
  ""specialFocus"": ""string""
}}";
            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage(systemPrompt),
                ChatMessage.CreateUserMessage($"CV Analysis:\n{cvAnalysis}\n\nJD Analysis:\n{jdAnalysis}")
            };

            var options = new ChatCompletionOptions { Temperature = 0.4f };
            var response = await _chatClient.CompleteChatAsync(messages, options, ct);
            var content = CleanJsonString(response.Value.Content[0].Text);

            using var doc = JsonDocument.Parse(content);
            var totalQuestions = doc.RootElement.TryGetProperty("totalQuestions", out var tq) ? tq.GetInt32() : 7;

            return InterviewPlanResult.Success(content, totalQuestions, Provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create interview plan.");
            return InterviewPlanResult.Failure(ex.Message, Provider);
        }
    }

    private string CleanJsonString(string text)
    {
        text = text.Trim();
        if (text.StartsWith("```json")) text = text.Substring(7);
        if (text.StartsWith("```")) text = text.Substring(3);
        if (text.EndsWith("```")) text = text.Substring(0, text.Length - 3);
        return text.Trim();
    }
}

public class NimInterviewConductorService : IInterviewConductorService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<NimInterviewConductorService> _logger;

    public ProviderKind Provider => ProviderKind.OpenAI;

    public NimInterviewConductorService(
        IOptions<OpenAIOptions> options,
        ILogger<NimInterviewConductorService> logger)
    {
        _logger = logger;
        var opts = options.Value;
        var clientOptions = NimClientOptionsFactory.Create(opts);
        _chatClient = new ChatClient(opts.ChatModel, new ApiKeyCredential(opts.ApiKey), clientOptions);
    }

    public async Task<InterviewQuestionResult> GenerateNextQuestionAsync(
        InterviewConductorContext context, CancellationToken ct = default)
    {
        try
        {
            var isLast = context.CurrentQuestionNumber >= context.PlannedQuestionCount;
            var systemPrompt = $@"You are a professional English-speaking interviewer conducting a mock interview for a Vietnamese IT professional.
Interview type: {context.InterviewType}
This is question {context.CurrentQuestionNumber} of {context.PlannedQuestionCount}.
{(isLast ? "This is the LAST question — make it a strong closing question." : "")}

Interview Plan: {context.InterviewPlan}
Candidate CV Analysis: {context.CvAnalysis}
JD Analysis: {context.JdAnalysis}

Based on the conversation so far, ask the next appropriate question.
If the candidate just answered, you may ask a brief follow-up before moving to the next topic.

Output ONLY valid JSON:
{{
  ""question"": ""Your interview question"",
  ""category"": ""Behavioral|Technical|Situational|FollowUp|Opening|Closing"",
  ""coachingHint"": ""A brief hint for the learner (optional, null if last question)"",
  ""isLastQuestion"": {(isLast ? "true" : "false")}
}}";
            var messages = new List<ChatMessage> { ChatMessage.CreateSystemMessage(systemPrompt) };

            foreach (var turn in context.ConversationHistory)
            {
                if (turn.Speaker == "Learner")
                    messages.Add(ChatMessage.CreateUserMessage(turn.Message));
                else
                    messages.Add(ChatMessage.CreateAssistantMessage(turn.Message));
            }

            if (context.LatestLearnerAnswer != null)
                messages.Add(ChatMessage.CreateUserMessage(context.LatestLearnerAnswer.Message));

            var options = new ChatCompletionOptions { Temperature = 0.6f };
            var response = await _chatClient.CompleteChatAsync(messages, options, ct);
            var content = CleanJsonString(response.Value.Content[0].Text);

            var parsed = JsonSerializer.Deserialize<InterviewQuestionContent>(
                content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (parsed is null)
                return InterviewQuestionResult.Failure("Failed to parse AI response.", Provider);

            return InterviewQuestionResult.Success(parsed, Provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate interview question.");
            return InterviewQuestionResult.Failure(ex.Message, Provider);
        }
    }

    public async Task<InterviewFeedbackResult> EvaluateSessionAsync(
        InterviewConductorContext context, CancellationToken ct = default)
    {
        try
        {
            var transcript = string.Join("\n", context.ConversationHistory.Select(t => $"{t.Speaker}: {t.Message}"));

            var systemPrompt = $@"You are an expert English interview coach evaluating a completed mock interview session for a Vietnamese IT professional.

Interview type: {context.InterviewType}
Interview Plan: {context.InterviewPlan}
CV Analysis: {context.CvAnalysis}
JD Analysis: {context.JdAnalysis}

Evaluate the candidate's performance and provide feedback in BOTH English and Vietnamese.
Score each area from 0-100.

Output ONLY valid JSON:
{{
  ""overallScore"": number,
  ""communicationScore"": number,
  ""technicalAccuracyScore"": number,
  ""confidenceScore"": number,
  ""detailedFeedbackEn"": ""Detailed English feedback"",
  ""detailedFeedbackVi"": ""Detailed Vietnamese feedback"",
  ""strengthAreas"": [""string""],
  ""improvementAreas"": [""string""],
  ""suggestedPhrases"": [""Useful English phrases the candidate should learn""],
  ""retryRecommendation"": ""Specific advice for next practice session""
}}";
            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage(systemPrompt),
                ChatMessage.CreateUserMessage($"Interview Transcript:\n{transcript}")
            };

            var options = new ChatCompletionOptions { Temperature = 0.3f };
            var response = await _chatClient.CompleteChatAsync(messages, options, ct);
            var content = CleanJsonString(response.Value.Content[0].Text);

            var parsed = JsonSerializer.Deserialize<InterviewFeedbackContent>(
                content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (parsed is null)
                return InterviewFeedbackResult.Failure("Failed to parse feedback.", Provider);

            return InterviewFeedbackResult.Success(parsed, Provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate interview session.");
            return InterviewFeedbackResult.Failure(ex.Message, Provider);
        }
    }

    private string CleanJsonString(string text)
    {
        text = text.Trim();
        if (text.StartsWith("```json")) text = text.Substring(7);
        if (text.StartsWith("```")) text = text.Substring(3);
        if (text.EndsWith("```")) text = text.Substring(0, text.Length - 3);
        return text.Trim();
    }
}
