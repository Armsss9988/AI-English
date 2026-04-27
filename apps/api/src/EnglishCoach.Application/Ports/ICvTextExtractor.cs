namespace EnglishCoach.Application.Ports;

public interface ICvTextExtractor
{
    Task<string> ExtractTextAsync(Stream fileStream, CancellationToken ct = default);
}
