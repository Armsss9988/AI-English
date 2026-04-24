using EnglishCoach.SharedKernel.Time;

namespace EnglishCoach.IntegrationTests;

internal sealed class FakeClock : IClock
{
    public FakeClock(DateTimeOffset utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTimeOffset UtcNow { get; }
    public DateOnly Today => DateOnly.FromDateTime(UtcNow.UtcDateTime);
}
