namespace EnglishCoach.Contracts.Identity;

public sealed record MyProfileResponse(
    string UserId,
    string DisplayName,
    string NativeLanguage,
    string Timezone,
    string CurrentEnglishLevel,
    string TargetUseCase,
    int TargetTimelineWeeks,
    string Role);
