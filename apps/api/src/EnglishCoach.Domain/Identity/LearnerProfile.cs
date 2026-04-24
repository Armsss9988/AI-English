namespace EnglishCoach.Domain.Identity;

public sealed class LearnerProfile
{
    private LearnerProfile()
    {
        UserId = string.Empty;
        DisplayName = string.Empty;
        NativeLanguage = string.Empty;
        Timezone = string.Empty;
        TargetUseCase = string.Empty;
    }

    private LearnerProfile(
        string userId,
        string displayName,
        string nativeLanguage,
        string timezone,
        EnglishLevel currentEnglishLevel,
        string targetUseCase,
        int targetTimelineWeeks,
        LearnerRole role)
    {
        UserId = Require(userId, nameof(userId), 128);
        ApplyProfileDetails(
            displayName,
            nativeLanguage,
            timezone,
            currentEnglishLevel,
            targetUseCase,
            targetTimelineWeeks,
            role);
    }

    public string UserId { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string NativeLanguage { get; private set; } = string.Empty;

    public string Timezone { get; private set; } = string.Empty;

    public EnglishLevel CurrentEnglishLevel { get; private set; }

    public string TargetUseCase { get; private set; } = string.Empty;

    public int TargetTimelineWeeks { get; private set; }

    public LearnerRole Role { get; private set; }

    public static LearnerProfile Create(
        string userId,
        string displayName,
        string nativeLanguage,
        string timezone,
        EnglishLevel currentEnglishLevel,
        string targetUseCase,
        int targetTimelineWeeks,
        LearnerRole role)
    {
        return new LearnerProfile(
            userId,
            displayName,
            nativeLanguage,
            timezone,
            currentEnglishLevel,
            targetUseCase,
            targetTimelineWeeks,
            role);
    }

    public void Update(
        string displayName,
        string nativeLanguage,
        string timezone,
        EnglishLevel currentEnglishLevel,
        string targetUseCase,
        int targetTimelineWeeks,
        LearnerRole role)
    {
        ApplyProfileDetails(
            displayName,
            nativeLanguage,
            timezone,
            currentEnglishLevel,
            targetUseCase,
            targetTimelineWeeks,
            role);
    }

    private void ApplyProfileDetails(
        string displayName,
        string nativeLanguage,
        string timezone,
        EnglishLevel currentEnglishLevel,
        string targetUseCase,
        int targetTimelineWeeks,
        LearnerRole role)
    {
        DisplayName = Require(displayName, nameof(displayName), 120);
        NativeLanguage = Require(nativeLanguage, nameof(nativeLanguage), 16);
        Timezone = Require(timezone, nameof(timezone), 64);
        TargetUseCase = Require(targetUseCase, nameof(targetUseCase), 240);

        if (targetTimelineWeeks <= 0 || targetTimelineWeeks > 260)
        {
            throw new ArgumentOutOfRangeException(nameof(targetTimelineWeeks), "Target timeline weeks must be between 1 and 260.");
        }

        CurrentEnglishLevel = currentEnglishLevel;
        TargetTimelineWeeks = targetTimelineWeeks;
        Role = role;
    }

    private static string Require(string value, string paramName, int maxLength)
    {
        var trimmed = value.Trim();

        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Value is required.", paramName);
        }

        if (trimmed.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Value must be {maxLength} characters or fewer.");
        }

        return trimmed;
    }
}
