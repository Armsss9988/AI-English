using EnglishCoach.Application.Ports;

namespace EnglishCoach.Infrastructure.AI.FakeAdapters;

public class FakeTranscriptionService : ISpeechTranscriptionService
{
    public ProviderKind Provider => ProviderKind.Fake;

    private readonly string _transcript;
    private readonly bool _shouldFail;
    private readonly string _errorCode;
    private readonly string _errorMessage;

    public FakeTranscriptionService(
        string transcript = "This is a sample transcription.",
        bool shouldFail = false,
        string errorCode = "TRANSCRIPTION_FAILED",
        string errorMessage = "Simulated transcription failure")
    {
        _transcript = transcript;
        _shouldFail = shouldFail;
        _errorCode = errorCode;
        _errorMessage = errorMessage;
    }

    public Task<TranscriptionResult> TranscribeAsync(AudioReference audio, CancellationToken ct = default)
    {
        if (_shouldFail)
        {
            return Task.FromResult(TranscriptionResult.Failure(_errorCode, _errorMessage, Provider));
        }

        return Task.FromResult(TranscriptionResult.Success(_transcript, Provider));
    }

    public static FakeTranscriptionService Success(string transcript = "This is a sample transcription.")
        => new(transcript);

    public static FakeTranscriptionService Failure(string errorCode = "TRANSCRIPTION_FAILED", string errorMessage = "Simulated failure")
        => new(shouldFail: true, errorCode: errorCode, errorMessage: errorMessage);
}