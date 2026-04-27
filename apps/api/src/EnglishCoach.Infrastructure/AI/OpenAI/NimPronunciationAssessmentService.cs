using System.ClientModel;
using System.Text.Json;
using EnglishCoach.Application.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace EnglishCoach.Infrastructure.AI.OpenAI;

/// <summary>
/// Real NIM/OpenAI adapter for Pronunciation Assessment.
/// Uses the chat model to analyze word-level STT confidence data and identify pronunciation issues.
/// This is an LLM-based approach since OpenAI doesn't provide a native pronunciation assessment API.
/// </summary>
public class NimPronunciationAssessmentService : IPronunciationAssessmentService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<NimPronunciationAssessmentService> _logger;

    public ProviderKind Provider => ProviderKind.OpenAI;

    public NimPronunciationAssessmentService(
        IOptions<OpenAIOptions> options,
        ILogger<NimPronunciationAssessmentService> logger)
    {
        _logger = logger;
        var opts = options.Value;
        var clientOptions = NimClientOptionsFactory.Create(opts);
        _chatClient = new ChatClient(opts.ChatModel, new ApiKeyCredential(opts.ApiKey), clientOptions);
    }

    public async Task<PronunciationAssessmentResult> AssessAsync(
        PronunciationAssessmentRequest request, CancellationToken ct = default)
    {
        try
        {
            // Build word confidence data for the prompt
            var wordConfidenceSection = "";
            if (request.WordConfidences is { Count: > 0 })
            {
                var wordData = request.WordConfidences.Select(w =>
                    $"  {{ \"word\": \"{w.Word}\", \"confidence\": {w.Confidence:F2}, \"startMs\": {w.StartMs}, \"endMs\": {w.EndMs} }}");
                wordConfidenceSection = $"Word-level STT data:\n[\n{string.Join(",\n", wordData)}\n]";
            }

            var importantTerms = request.ImportantTerms.Count > 0
                ? $"Important technical terms to check: {string.Join(", ", request.ImportantTerms)}"
                : "";

            var systemPrompt = $@"You are an expert English pronunciation coach for Vietnamese IT professionals.
Analyze the learner's pronunciation based on STT (speech-to-text) word confidence data.

The learner is answering an interview question. Focus on:
1. Words with low STT confidence (< 0.7) — likely pronunciation issues
2. Common Vietnamese pronunciation pitfalls: ending consonants (s/t/d), word stress, vowel sounds
3. Technical terms that need precise pronunciation

Question asked: {request.QuestionText}
Raw transcript (what STT heard): {request.RawTranscript}
Confirmed transcript (what learner intended): {request.ConfirmedTranscript}
{wordConfidenceSection}
{importantTerms}

Score each area from 0-100:
- overall: general pronunciation quality
- fluency: speaking flow and rhythm
- accuracy: individual word pronunciation
- completeness: whether the answer was fully spoken (not cut off)

Identify specific word issues for Vietnamese speakers.

Output ONLY valid JSON:
{{
  ""overallScore"": number (0-100),
  ""fluencyScore"": number (0-100),
  ""accuracyScore"": number (0-100),
  ""completenessScore"": number (0-100),
  ""wordIssues"": [
    {{
      ""heardAs"": ""what STT heard"",
      ""expected"": ""correct pronunciation"",
      ""issueType"": ""EndingSound|WordStress|Vowel|TechnicalTerm|Consonant|Intonation"",
      ""explanationVi"": ""Vietnamese explanation for the learner"",
      ""correctPronunciationText"": ""how to say it correctly"",
      ""ipa"": ""/phonetic/"",
      ""severity"": ""Low|Medium|High""
    }}
  ]
}}";

            var messages = new List<ChatMessage>
            {
                ChatMessage.CreateSystemMessage(systemPrompt),
                ChatMessage.CreateUserMessage("Analyze this pronunciation data and provide feedback.")
            };

            var options = new ChatCompletionOptions { Temperature = 0.2f };
            var response = await _chatClient.CompleteChatAsync(messages, options, ct);
            var content = CleanJsonString(response.Value.Content[0].Text);

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            var overallScore = root.TryGetProperty("overallScore", out var os) ? os.GetInt32() : 70;
            var fluencyScore = root.TryGetProperty("fluencyScore", out var fs) ? fs.GetInt32() : 70;
            var accuracyScore = root.TryGetProperty("accuracyScore", out var acs) ? acs.GetInt32() : 70;
            var completenessScore = root.TryGetProperty("completenessScore", out var cs) ? cs.GetInt32() : 80;

            var issues = new List<PronunciationWordIssue>();
            if (root.TryGetProperty("wordIssues", out var wi) && wi.ValueKind == JsonValueKind.Array)
            {
                foreach (var issue in wi.EnumerateArray())
                {
                    issues.Add(new PronunciationWordIssue
                    {
                        HeardAs = issue.TryGetProperty("heardAs", out var ha) ? ha.GetString() ?? "" : "",
                        Expected = issue.TryGetProperty("expected", out var exp) ? exp.GetString() ?? "" : "",
                        IssueType = issue.TryGetProperty("issueType", out var it) ? it.GetString() ?? "Vowel" : "Vowel",
                        ExplanationVi = issue.TryGetProperty("explanationVi", out var ev) ? ev.GetString() ?? "" : "",
                        CorrectPronunciationText = issue.TryGetProperty("correctPronunciationText", out var cpt) ? cpt.GetString() ?? "" : "",
                        Ipa = issue.TryGetProperty("ipa", out var ipa) && ipa.ValueKind != JsonValueKind.Null ? ipa.GetString() : null,
                        Severity = issue.TryGetProperty("severity", out var sev) ? sev.GetString() ?? "Medium" : "Medium"
                    });
                }
            }

            _logger.LogInformation("Pronunciation assessment: overall={Score}, issues={Count}",
                overallScore, issues.Count);

            return PronunciationAssessmentResult.Success(
                overallScore, fluencyScore, accuracyScore, completenessScore,
                issues, Provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NIM pronunciation assessment failed, using fallback.");
            return GenerateFallback(request);
        }
    }

    private PronunciationAssessmentResult GenerateFallback(PronunciationAssessmentRequest request)
    {
        var issues = new List<PronunciationWordIssue>();

        // Flag low-confidence words from STT data
        if (request.WordConfidences is { Count: > 0 })
        {
            foreach (var word in request.WordConfidences.Where(w => w.Confidence < 0.7))
            {
                issues.Add(new PronunciationWordIssue
                {
                    HeardAs = word.Word,
                    Expected = word.Word,
                    IssueType = "LowConfidenceWord",
                    ExplanationVi = $"Từ '{word.Word}' chưa được phát âm rõ ràng (confidence: {word.Confidence:P0}).",
                    CorrectPronunciationText = word.Word,
                    Severity = word.Confidence < 0.5 ? "High" : "Medium"
                });
            }
        }

        return PronunciationAssessmentResult.Success(
            overall: 70, fluency: 68, accuracy: 72, completeness: 80,
            issues, Provider, fallback: true);
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
