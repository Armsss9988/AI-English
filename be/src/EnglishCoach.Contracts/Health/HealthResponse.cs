namespace EnglishCoach.Contracts.Health;

public record HealthResponse(
    string Status,
    DateTimeOffset Timestamp,
    string Version
);

public static class HealthResponseFactory
{
    public static HealthResponse Create(string version = "1.0.0") =>
        new("healthy", DateTimeOffset.UtcNow, version);
}