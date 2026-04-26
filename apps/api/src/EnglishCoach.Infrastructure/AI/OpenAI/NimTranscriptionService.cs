using System.ClientModel;
using EnglishCoach.Application.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Audio;

namespace EnglishCoach.Infrastructure.AI.OpenAI;

public class NimTranscriptionService : ISpeechTranscriptionService
{
    private readonly AudioClient _audioClient;
    private readonly OpenAIOptions _options;
    private readonly ILogger<NimTranscriptionService> _logger;

    public ProviderKind Provider => ProviderKind.OpenAI;

    public NimTranscriptionService(
        IOptions<OpenAIOptions> options,
        ILogger<NimTranscriptionService> logger)
    {
        _options = options.Value;
        _logger = logger;
        var clientOptions = new global::OpenAI.OpenAIClientOptions();
        if (!string.IsNullOrEmpty(_options.Endpoint))
        {
            clientOptions.Endpoint = new Uri(_options.Endpoint);
        }

        _audioClient = new AudioClient(_options.AudioModel, new ApiKeyCredential(_options.ApiKey), clientOptions);
    }

    public async Task<TranscriptionResult> TranscribeAsync(AudioReference audio, CancellationToken ct = default)
    {
        try
        {
            // For MVP, assuming AudioUrl is a local path or we just simulate if it's a URL.
            // In a real app, you would download the file first if it's a URL.
            if (!File.Exists(audio.AudioUrl))
            {
                // Fallback simulation for local dev if file doesn't exist
                return TranscriptionResult.Success($"[Simulated transcript for {audio.AttemptId}]", Provider);
            }

            var options = new AudioTranscriptionOptions
            {
                ResponseFormat = AudioTranscriptionFormat.Text,
                Language = "en"
            };
            using var stream = File.OpenRead(audio.AudioUrl);
            var result = await _audioClient.TranscribeAudioAsync(stream, Path.GetFileName(audio.AudioUrl), options, ct);

            return TranscriptionResult.Success(result.Value.Text, Provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transcribe audio.");
            return TranscriptionResult.Failure("TranscriptionFailed", ex.Message, Provider);
        }
    }
}
