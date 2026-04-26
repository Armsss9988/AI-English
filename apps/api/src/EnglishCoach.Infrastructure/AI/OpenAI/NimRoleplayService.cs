using System.ClientModel;
using System.Text.Json;
using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.Roleplay;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace EnglishCoach.Infrastructure.AI.OpenAI;

public class NimRoleplayService : IRoleplayResponseService
{
    private readonly ChatClient _chatClient;
    private readonly OpenAIOptions _options;
    private readonly ILogger<NimRoleplayService> _logger;

    public ProviderKind Provider => ProviderKind.OpenAI;

    public NimRoleplayService(
        IOptions<OpenAIOptions> options,
        ILogger<NimRoleplayService> logger)
    {
        _options = options.Value;
        _logger = logger;
        var clientOptions = new global::OpenAI.OpenAIClientOptions();
        if (!string.IsNullOrEmpty(_options.Endpoint))
        {
            clientOptions.Endpoint = new Uri(_options.Endpoint);
        }

        _chatClient = new ChatClient(_options.ChatModel, new ApiKeyCredential(_options.ApiKey), clientOptions);
    }

    public async Task<RoleplayResult> GenerateResponseAsync(RoleplayContext context, CancellationToken ct = default)
    {
        try
        {
            var systemPrompt = $@"You are acting as: {context.ScenarioPersona}.
Scenario: {context.ScenarioTitle}
Goal: {context.ScenarioGoal}
Difficulty: {context.Difficulty}/5

Respond naturally to the learner as this persona. 
You must output ONLY valid JSON matching this structure:
{{
  ""clientMessage"": ""Your response as the persona"",
  ""coachingNote"": ""A brief tip for the learner (optional)"",
  ""isSessionComplete"": true/false,
  ""evaluatedCriteria"": [""Criteria met in this turn""]
}}";

            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage(systemPrompt)
            };

            foreach (var turn in context.ConversationHistory)
            {
                if (turn.Speaker == "Learner")
                    messages.Add(ChatMessage.CreateUserMessage(turn.Message));
                else
                    messages.Add(ChatMessage.CreateAssistantMessage(turn.Message));
            }

            if (context.LatestLearnerTurn != null)
            {
                messages.Add(ChatMessage.CreateUserMessage(context.LatestLearnerTurn.Message));
            }

            var options = new ChatCompletionOptions { Temperature = 0.7f };
            var response = await _chatClient.CompleteChatAsync(messages, options, ct);
            var content = response.Value.Content[0].Text;

            // Clean json markdown
            content = CleanJsonString(content);

            var roleplayResponse = JsonSerializer.Deserialize<RoleplayResponseContent>(
                content, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (roleplayResponse is null)
                return RoleplayResult.Failure("ParseError", "Failed to parse JSON response.", Provider);

            return RoleplayResult.Success(roleplayResponse, Provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate roleplay response.");
            return RoleplayResult.Failure("RoleplayFailed", ex.Message, Provider);
        }
    }

    public async Task<RoleplaySummary> EvaluateSessionAsync(RoleplayContext context, CancellationToken ct = default)
    {
        try
        {
            var transcript = string.Join("\n", context.ConversationHistory.Select(t => $"{t.Speaker}: {t.Message}"));
            
            var systemPrompt = $@"You are an expert English coach evaluating a completed roleplay session.
Scenario: {context.ScenarioTitle}
Goal: {context.ScenarioGoal}
Success Criteria: {string.Join(", ", context.SuccessCriteria)}

Review the transcript and output ONLY valid JSON matching this structure:
{{
  ""status"": ""Passed/Failed"",
  ""overallFeedback"": ""string"",
  ""grammarFeedback"": ""string"",
  ""suggestedAlternative"": ""string"",
  ""targetPhrase"": ""string"",
  ""confidenceNote"": ""string""
}}";

            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage(systemPrompt),
                ChatMessage.CreateUserMessage($"Transcript:\n{transcript}")
            };

            var options = new ChatCompletionOptions { Temperature = 0.3f };
            var response = await _chatClient.CompleteChatAsync(messages, options, ct);
            var content = CleanJsonString(response.Value.Content[0].Text);

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            
            return new RoleplaySummary(
                GetString(root, "status", "Passed"),
                GetString(root, "overallFeedback", "Good effort."),
                GetString(root, "grammarFeedback", "Grammar was mostly correct."),
                GetString(root, "suggestedAlternative", ""),
                GetString(root, "targetPhrase", ""),
                GetString(root, "confidenceNote", "")
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate roleplay session.");
            return new RoleplaySummary("Failed", "Evaluation failed due to an error.", "", "", "", "");
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

    private string GetString(JsonElement element, string prop, string defaultVal)
    {
        if (element.TryGetProperty(prop, out var val))
            return val.GetString() ?? defaultVal;
        return defaultVal;
    }
}
