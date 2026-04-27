using EnglishCoach.Application.Ports;

namespace EnglishCoach.Infrastructure.Storage;

/// <summary>
/// Local filesystem audio storage for development.
/// Production should use cloud storage (S3/Azure Blob).
/// </summary>
public sealed class LocalInterviewAudioStorage : IInterviewAudioStorage
{
    private readonly string _basePath;

    public LocalInterviewAudioStorage(string basePath)
    {
        _basePath = basePath;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<AudioStorageResult> SaveAsync(AudioStorageRequest request, CancellationToken ct = default)
    {
        try
        {
            var sessionDir = Path.Combine(_basePath, request.SessionId);
            Directory.CreateDirectory(sessionDir);

            var extension = GetExtension(request.ContentType);
            var fileName = $"{request.Purpose}_{request.TurnId}{extension}";
            var filePath = Path.Combine(sessionDir, fileName);
            var storageKey = $"{request.SessionId}/{fileName}";

            await File.WriteAllBytesAsync(filePath, request.AudioData, ct);

            return AudioStorageResult.Success(storageKey, request.AudioData.Length);
        }
        catch (Exception ex)
        {
            return AudioStorageResult.Failure($"Failed to save audio: {ex.Message}");
        }
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct = default)
    {
        var filePath = Path.Combine(_basePath, storageKey);
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Audio file not found: {storageKey}");

        Stream stream = File.OpenRead(filePath);
        return Task.FromResult(stream);
    }

    public Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default)
    {
        var filePath = Path.Combine(_basePath, storageKey);
        return Task.FromResult(File.Exists(filePath));
    }

    private static string GetExtension(string contentType) => contentType switch
    {
        "audio/webm" => ".webm",
        "audio/wav" => ".wav",
        "audio/mp4" => ".mp4",
        "audio/mp3" => ".mp3",
        "audio/mpeg" => ".mp3",
        _ => ".bin"
    };
}
