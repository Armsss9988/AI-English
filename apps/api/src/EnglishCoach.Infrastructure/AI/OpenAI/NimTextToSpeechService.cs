using System.ClientModel;
using EnglishCoach.Application.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Audio;

namespace EnglishCoach.Infrastructure.AI.OpenAI;

/// <summary>
/// Real NIM/OpenAI adapter for Text-to-Speech.
/// Converts interviewer question text into spoken audio using OpenAI TTS API.
/// Falls back to empty audio if the API is unavailable (interview continues text-only).
/// </summary>
public class NimTextToSpeechService : ITextToSpeechService
{
    private readonly AudioClient _audioClient;
    private readonly ILogger<NimTextToSpeechService> _logger;

    public ProviderKind Provider => ProviderKind.OpenAI;

    // Voice mapping: purpose → OpenAI voice name
    private static readonly Dictionary<string, string> VoiceMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["InterviewerTurn"] = "alloy",        // professional, neutral
        ["PronunciationExample"] = "nova",    // clear, slower
    };

    public NimTextToSpeechService(
        IOptions<OpenAIOptions> options,
        ILogger<NimTextToSpeechService> logger)
    {
        _logger = logger;
        var opts = options.Value;
        // TTS uses a dedicated model name; fall back to "tts-1" if AudioModel isn't TTS-specific
        var ttsModel = opts.AudioModel.StartsWith("tts", StringComparison.OrdinalIgnoreCase)
            ? opts.AudioModel
            : "tts-1";

        var clientOptions = new global::OpenAI.OpenAIClientOptions();
        if (!string.IsNullOrEmpty(opts.Endpoint))
            clientOptions.Endpoint = new Uri(opts.Endpoint);

        _audioClient = new AudioClient(ttsModel, new ApiKeyCredential(opts.ApiKey), clientOptions);
    }

    public async Task<TextToSpeechResult> SynthesizeAsync(TextToSpeechRequest request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return TextToSpeechResult.Failure("Empty text for TTS.", Provider);
            }

            // Select voice based on purpose
            var voiceName = VoiceMap.GetValueOrDefault(request.Purpose, "alloy");
            var voice = voiceName switch
            {
                "nova" => GeneratedSpeechVoice.Nova,
                "echo" => GeneratedSpeechVoice.Echo,
                "fable" => GeneratedSpeechVoice.Fable,
                "onyx" => GeneratedSpeechVoice.Onyx,
                "shimmer" => GeneratedSpeechVoice.Shimmer,
                _ => GeneratedSpeechVoice.Alloy,
            };

            var ttsOptions = new SpeechGenerationOptions
            {
                SpeedRatio = (float)request.SpeakingRate,
                ResponseFormat = GeneratedSpeechFormat.Mp3,
            };

            var result = await _audioClient.GenerateSpeechAsync(
                request.Text, voice, ttsOptions, ct);

            var audioData = result.Value.ToArray();

            if (audioData.Length == 0)
            {
                return TextToSpeechResult.Failure("TTS returned empty audio.", Provider);
            }

            // Estimate duration: ~150 words per minute for English speech
            var wordCount = request.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            var estimatedDurationMs = (int)(wordCount / 2.5 * 1000 / request.SpeakingRate);

            _logger.LogInformation("TTS success: {ByteSize} bytes, ~{DurationMs}ms, voice={Voice}",
                audioData.Length, estimatedDurationMs, voiceName);

            return TextToSpeechResult.Success(audioData, "audio/mp3", estimatedDurationMs, Provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NIM TTS failed for text length {Length}", request.Text?.Length ?? 0);

            // Graceful degradation: return failure (interview continues text-only)
            return TextToSpeechResult.Failure(
                $"TTS service temporarily unavailable: {ex.Message}", Provider);
        }
    }
}
