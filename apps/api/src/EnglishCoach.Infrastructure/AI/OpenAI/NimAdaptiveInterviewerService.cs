using System.ClientModel;
using System.Text.Json;
using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.InterviewPractice;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace EnglishCoach.Infrastructure.AI.OpenAI;

/// <summary>
/// Real NIM/OpenAI adapter for adaptive interview turn generation and answer evaluation.
/// Uses structured JSON output from the chat model.
/// </summary>
public class NimAdaptiveInterviewerService : IAdaptiveInterviewerService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<NimAdaptiveInterviewerService> _logger;
    private readonly string _modelId;

    public ProviderKind Provider => ProviderKind.OpenAI;

    public NimAdaptiveInterviewerService(
        IOptions<OpenAIOptions> options,
        ILogger<NimAdaptiveInterviewerService> logger)
    {
        _logger = logger;
        var opts = options.Value;
        _modelId = opts.ChatModel;
        var clientOptions = NimClientOptionsFactory.Create(opts);
        _chatClient = new ChatClient(opts.ChatModel, new ApiKeyCredential(opts.ApiKey), clientOptions);
    }

    public async Task<InterviewTurnGenerationResult> GenerateInterviewerTurnAsync(
        InterviewTurnGenerationContext context, CancellationToken ct = default)
    {
        try
        {
            var isLast = context.CurrentQuestionNumber >= context.PlannedQuestionCount;
            var modeText = context.InterviewMode == InterviewMode.TrainingInterview
                ? "TRAINING mode — include a coaching hint in Vietnamese-developer-friendly English to guide the learner."
                : "REAL INTERVIEW mode — do NOT include any coaching hint.";

            var systemPrompt = $@"You are an expert English-speaking interviewer conducting a mock interview for a Vietnamese IT professional.
You are context-aware and adaptive: you choose the next question based on the candidate's previous answers, their CV, and the JD.

Interview Mode: {modeText}
This is question {context.CurrentQuestionNumber} of {context.PlannedQuestionCount}.
{(isLast ? "This is the LAST question — make it a strong closing question." : "")}

Interview Plan: {context.InterviewPlan}
Candidate CV Analysis: {context.CvAnalysis}
JD Analysis: {context.JdAnalysis}
{(context.CurrentCapabilityTarget.HasValue ? $"Target capability: {context.CurrentCapabilityTarget}" : "")}
{(context.PreviousTurnDecision != null ? $"Previous decision: {context.PreviousTurnDecision}" : "")}
{(context.LatestScorecardJson != null ? $"Latest scorecard: {context.LatestScorecardJson}" : "")}
{(context.LatestPronunciationReportJson != null ? $"Pronunciation report: {context.LatestPronunciationReportJson}" : "")}

Decision rules:
- If the latest answer was short/vague (< 25 words), ask a FollowUp to get more detail. Do NOT advance the plan.
- If the learner's pronunciation was poor, ask a Clarification. Do NOT advance the plan.
- If the answer scored below 40 overall and this is Training mode, consider a Challenge to push deeper.
- Otherwise, advance to the next MainQuestion on the plan.

Output ONLY valid JSON with this exact structure:
{{
  ""question"": ""Your interview question text"",
  ""turnType"": ""OpeningQuestion|MainQuestion|FollowUp|Clarification|Challenge|Transition|Closing"",
  ""targetCapability"": ""SelfIntroduction|ProjectDeepDive|TechnicalTradeoff|BehavioralStar|ClientCommunication|RequirementClarification|IncidentConflictStory|WeakSpotRetry|EnglishClarity|PronunciationClarity"",
  ""shouldAdvancePlan"": true/false,
  ""reasonCode"": ""normal_progression|shallow_answer|low_pronunciation_confidence|low_score_retry|last_question"",
  ""learnerFacingHint"": ""coaching hint or null"",
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

            if (!string.IsNullOrWhiteSpace(context.LatestLearnerTranscript))
                messages.Add(ChatMessage.CreateUserMessage(context.LatestLearnerTranscript));

            var options = new ChatCompletionOptions { Temperature = 0.6f };
            var response = await _chatClient.CompleteChatAsync(messages, options, ct);
            var content = CleanJsonString(response.Value.Content[0].Text);

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            var question = root.GetProperty("question").GetString() ?? "Could you tell me more?";
            var turnTypeStr = root.TryGetProperty("turnType", out var tt) ? tt.GetString() : "MainQuestion";
            var capabilityStr = root.TryGetProperty("targetCapability", out var tc) ? tc.GetString() : "EnglishClarity";
            var shouldAdvance = root.TryGetProperty("shouldAdvancePlan", out var sa) && sa.GetBoolean();
            var reasonCode = root.TryGetProperty("reasonCode", out var rc) ? rc.GetString() ?? "normal_progression" : "normal_progression";
            var hint = root.TryGetProperty("learnerFacingHint", out var lh) && lh.ValueKind != JsonValueKind.Null ? lh.GetString() : null;
            var isLastFromAi = root.TryGetProperty("isLastQuestion", out var il) && il.GetBoolean();

            var turnType = Enum.TryParse<InterviewTurnType>(turnTypeStr, ignoreCase: true, out var parsedTt) ? parsedTt : InterviewTurnType.MainQuestion;
            var capability = Enum.TryParse<InterviewCapability>(capabilityStr, ignoreCase: true, out var parsedCap) ? parsedCap : InterviewCapability.EnglishClarity;

            return InterviewTurnGenerationResult.Success(
                question, turnType, capability, rubric: null,
                shouldAdvance, reasonCode, hint,
                isLast || isLastFromAi, Provider, _modelId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NIM adaptive interviewer turn generation failed, using fallback.");
            return GenerateFallbackTurn(context);
        }
    }

    public async Task<AnswerEvaluationResult> EvaluateAnswerAsync(
        AnswerEvaluationContext context, CancellationToken ct = default)
    {
        try
        {
            var systemPrompt = $@"You are an expert English interview evaluator scoring a Vietnamese IT professional's answer.
You must evaluate the answer against the question, the candidate's CV, and the JD requirements.

Question: {context.QuestionText}
Target Capability: {context.TargetCapability}
CV Analysis: {context.CvAnalysis}
JD Analysis: {context.JdAnalysis}
{(context.Rubric != null ? $"Rubric: {context.Rubric.SuccessCriteria}" : "")}
{(context.PronunciationReportJson != null ? $"Pronunciation: {context.PronunciationReportJson}" : "")}
Interview Mode: {context.InterviewMode}

Score each dimension from 0 to 10. Provide corrections for grammar/vocabulary mistakes.
Suggest a better answer. Identify useful phrases the learner could adopt.

Output ONLY valid JSON:
{{
  ""contentFitScore"": number (0-10),
  ""jdRelevanceScore"": number (0-10),
  ""cvEvidenceScore"": number (0-10),
  ""structureScore"": number (0-10),
  ""technicalCredibilityScore"": number (0-10),
  ""englishClarityScore"": number (0-10),
  ""professionalToneScore"": number (0-10),
  ""pronunciationClarityScore"": number (0-10),
  ""fluencyScore"": number (0-10),
  ""overallScore"": number (0-100),
  ""evidence"": ""What the candidate demonstrated well"",
  ""missingEvidence"": ""What was missing"",
  ""betterAnswer"": ""A model answer the learner can study"",
  ""corrections"": [{{ ""original"": ""wrong phrase"", ""corrected"": ""correct phrase"", ""explanationVi"": ""Vietnamese explanation"" }}],
  ""retryDrillPrompt"": ""Specific prompt for retry practice"",
  ""phraseCandidates"": [""useful phrases""],
  ""mistakeCandidates"": [""identified mistakes""],
  ""requiresRetry"": true/false
}}";

            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage(systemPrompt),
                ChatMessage.CreateUserMessage($"Candidate's answer:\n{context.ConfirmedTranscript}")
            };

            var options = new ChatCompletionOptions { Temperature = 0.3f };
            var response = await _chatClient.CompleteChatAsync(messages, options, ct);
            var content = CleanJsonString(response.Value.Content[0].Text);

            var parsed = JsonSerializer.Deserialize<AnswerScorecardContent>(
                content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (parsed is null)
                return AnswerEvaluationResult.Failure("Failed to parse evaluation response.", Provider);

            return AnswerEvaluationResult.Success(parsed, Provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NIM answer evaluation failed, using fallback.");
            return GenerateFallbackEvaluation(context);
        }
    }

    private InterviewTurnGenerationResult GenerateFallbackTurn(InterviewTurnGenerationContext context)
    {
        var isLast = context.CurrentQuestionNumber >= context.PlannedQuestionCount;
        return InterviewTurnGenerationResult.Success(
            "Could you tell me about a recent project where you had to solve a challenging technical problem?",
            InterviewTurnType.MainQuestion,
            InterviewCapability.ProjectDeepDive,
            rubric: null,
            shouldAdvance: true,
            reasonCode: "fallback",
            hint: context.InterviewMode == InterviewMode.TrainingInterview ? "Use specific examples with measurable outcomes." : null,
            isLast,
            Provider, _modelId, usedFallback: true);
    }

    private AnswerEvaluationResult GenerateFallbackEvaluation(AnswerEvaluationContext context)
    {
        var wordCount = context.ConfirmedTranscript.Split(' ').Length;
        var baseScore = Math.Min(100, 40 + wordCount * 2);
        var card = new AnswerScorecardContent
        {
            ContentFitScore = Math.Min(10, baseScore / 10),
            JdRelevanceScore = Math.Min(10, baseScore / 10 - 1),
            CvEvidenceScore = Math.Min(10, baseScore / 10 - 1),
            StructureScore = Math.Min(10, baseScore / 10),
            TechnicalCredibilityScore = Math.Min(10, baseScore / 10),
            EnglishClarityScore = Math.Min(10, baseScore / 10 - 2),
            ProfessionalToneScore = Math.Min(10, baseScore / 10),
            PronunciationClarityScore = Math.Min(10, baseScore / 10 - 2),
            FluencyScore = Math.Min(10, baseScore / 10 - 1),
            OverallScore = baseScore - 8,
            Evidence = "Answer provided.",
            MissingEvidence = "Detailed evaluation unavailable (AI provider timeout).",
            BetterAnswer = "Please try again for a full evaluation.",
            RetryDrillPrompt = "Try answering with more specific examples.",
            RequiresRetry = baseScore < 60
        };
        return AnswerEvaluationResult.Success(card, Provider, fallback: true);
    }

    private static string CleanJsonString(string text)
    {
        text = text.Trim();
        if (text.StartsWith("```json")) text = text[7..];
        if (text.StartsWith("```")) text = text[3..];
        if (text.EndsWith("```")) text = text[..^3];
        return text.Trim();
    }
}
