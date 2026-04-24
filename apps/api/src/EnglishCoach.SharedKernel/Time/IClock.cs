namespace EnglishCoach.SharedKernel.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
