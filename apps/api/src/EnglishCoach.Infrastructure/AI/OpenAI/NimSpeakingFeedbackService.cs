using System.ClientModel;
using System.Text.Json;
using EnglishCoach.Application.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace EnglishCoach.Infrastructure.AI.OpenAI;

public class NimSpeakingFeedbackService : ISpeakingFeedbackService
{
    private readonly ChatClient _chatClient;
    private readonly OpenAIOptions _options;
    private readonly ILogger<NimSpeakingFeedbackService> _logger;

    public ProviderKind Provider => ProviderKind.OpenAI;

    public NimSpeakingFeedbackService(
        IOptions<OpenAIOptions> options,
        ILogger<NimSpeakingFeedbackService> logger)
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

    public async Task<FeedbackResult> GenerateFeedbackAsync(SpeakingAttemptForEvaluation attempt, CancellationToken ct = default)
    {
        try
        {
            var systemPrompt = @"You are an expert English speaking coach. 
Evaluate the following learner's speech transcript.
You must output ONLY valid JSON matching this structure:
{
  ""pronunciationScore"": ""A/B/C/D/F"",
  ""fluencyScore"": ""A/B/C/D/F"",
  ""overallFeedback"": ""string"",
  ""areasToImprove"": [""string""],
  ""strengths"": [""string""]
}";

            var userPrompt = $"Transcript to evaluate:\n{attempt.Transcript}";

            var options = new ChatCompletionOptions
            {
                Temperature = 0.3f
            };

            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage(systemPrompt),
                ChatMessage.CreateUserMessage(userPrompt)
            };

            var response = await _chatClient.CompleteChatAsync(messages, options, ct);
            var content = response.Value.Content[0].Text;
            
            // Clean markdown JSON ticks if present
            content = content.Trim();
            if (content.StartsWith("```json")) content = content.Substring(7);
            if (content.StartsWith("```")) content = content.Substring(3);
            if (content.EndsWith("```")) content = content.Substring(0, content.Length - 3);

            var feedbackContent = JsonSerializer.Deserialize<SpeakingFeedbackContent>(
                content.Trim(), 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (feedbackContent is null)
            {
                return FeedbackResult.Failure("ParseError", "Failed to parse JSON response.", Provider);
            }

            return FeedbackResult.Success(feedbackContent, Provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate speaking feedback.");
            return FeedbackResult.Failure("FeedbackFailed", ex.Message, Provider);
        }
    }
}
