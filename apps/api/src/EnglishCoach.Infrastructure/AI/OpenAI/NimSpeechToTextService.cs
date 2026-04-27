using System.ClientModel;
using EnglishCoach.Application.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Audio;

namespace EnglishCoach.Infrastructure.AI.OpenAI;

/// <summary>
/// Real NIM/OpenAI adapter for interview voice pipeline Speech-to-Text.
/// Uses OpenAI Whisper API (or compatible endpoint) for audio transcription.
/// Falls back to a deterministic result if the API is unavailable.
/// </summary>
public class NimSpeechToTextService : ISpeechToTextService
{
    private readonly IInterviewAudioStorage _audioStorage;
    private readonly AudioClient _audioClient;
    private readonly ILogger<NimSpeechToTextService> _logger;

    public ProviderKind Provider => ProviderKind.OpenAI;

    public NimSpeechToTextService(
        IOptions<OpenAIOptions> options,
        IInterviewAudioStorage audioStorage,
        ILogger<NimSpeechToTextService> logger)
    {
        _audioStorage = audioStorage;
        _logger = logger;
        var opts = options.Value;
        var clientOptions = new global::OpenAI.OpenAIClientOptions();
        if (!string.IsNullOrEmpty(opts.Endpoint))
            clientOptions.Endpoint = new Uri(opts.Endpoint);
        _audioClient = new AudioClient(opts.AudioModel, new ApiKeyCredential(opts.ApiKey), clientOptions);
    }

    public async Task<SpeechToTextResult> TranscribeAsync(SpeechToTextRequest request, CancellationToken ct = default)
    {
        try
        {
            // Check if audio file exists in storage
            if (!await _audioStorage.ExistsAsync(request.AudioStorageKey, ct))
            {
                _logger.LogWarning("Audio file not found: {Key}", request.AudioStorageKey);
                return SpeechToTextResult.Failure("Audio file not found in storage.", Provider);
            }

            using var audioStream = await _audioStorage.OpenReadAsync(request.AudioStorageKey, ct);

            // Determine file extension from content type
            var extension = request.ContentType switch
            {
                "audio/webm" => "recording.webm",
                "audio/wav" => "recording.wav",
                "audio/mp3" or "audio/mpeg" => "recording.mp3",
                "audio/ogg" => "recording.ogg",
                "audio/flac" => "recording.flac",
                "audio/m4a" => "recording.m4a",
                _ => "recording.webm"
            };

            var transcriptionOptions = new AudioTranscriptionOptions
            {
                ResponseFormat = AudioTranscriptionFormat.Verbose,
                Language = request.LanguageHint ?? "en",
            };

            var result = await _audioClient.TranscribeAudioAsync(
                audioStream, extension, transcriptionOptions, ct);

            var transcript = result.Value.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(transcript))
            {
                return SpeechToTextResult.Failure("Transcription returned empty text.", Provider);
            }

            // Extract word timings if available
            // Word-level timing: Whisper verbose format may include segments
            var wordTimings = new List<WordTiming>();
            // Estimate word timings from total duration if available
            var words = transcript.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 0 && result.Value.Duration.HasValue)
            {
                var totalMs = (int)result.Value.Duration.Value.TotalMilliseconds;
                var perWordMs = totalMs / words.Length;
                for (int i = 0; i < words.Length; i++)
                {
                    wordTimings.Add(new WordTiming
                    {
                        Word = words[i],
                        Confidence = 0.9,
                        StartMs = i * perWordMs,
                        EndMs = (i + 1) * perWordMs,
                    });
                }
            }

            // Overall confidence: Whisper doesn't return a global score, estimate from result quality
            var confidence = transcript.Length > 10 ? 0.88 : 0.65;

            _logger.LogInformation("STT success: {CharCount} chars, {WordCount} words",
                transcript.Length, wordTimings.Count);

            return SpeechToTextResult.Success(
                transcript, confidence, Provider,
                wordTimings.Count > 0 ? wordTimings : null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NIM STT failed for {Key}", request.AudioStorageKey);

            // Return a fallback instead of failing hard
            return SpeechToTextResult.Success(
                "[Transcription temporarily unavailable. Please type your answer instead.]",
                confidence: 0.1,
                Provider,
                usedFallback: true);
        }
    }
}
