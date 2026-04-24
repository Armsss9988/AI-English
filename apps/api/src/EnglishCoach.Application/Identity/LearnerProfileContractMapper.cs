using EnglishCoach.Contracts.Identity;
using EnglishCoach.Domain.Identity;

namespace EnglishCoach.Application.Identity;

internal static class LearnerProfileContractMapper
{
    public static MyProfileResponse ToResponse(LearnerProfile profile)
    {
        return new MyProfileResponse(
            profile.UserId,
            profile.DisplayName,
            profile.NativeLanguage,
            profile.Timezone,
            profile.CurrentEnglishLevel.ToString(),
            profile.TargetUseCase,
            profile.TargetTimelineWeeks,
            profile.Role.ToString().ToLowerInvariant());
    }
}
