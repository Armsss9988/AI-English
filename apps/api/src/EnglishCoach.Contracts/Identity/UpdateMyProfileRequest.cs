using System.ComponentModel.DataAnnotations;

namespace EnglishCoach.Contracts.Identity;

public sealed record UpdateMyProfileRequest(
    [property: Required, StringLength(120, MinimumLength = 1)]
    string DisplayName,
    [property: Required, StringLength(16, MinimumLength = 2)]
    string NativeLanguage,
    [property: Required, StringLength(64, MinimumLength = 1)]
    string Timezone,
    [property: Required, RegularExpression("A1|A2|B1|B2|C1|C2")]
    string CurrentEnglishLevel,
    [property: Required, StringLength(240, MinimumLength = 1)]
    string TargetUseCase,
    [property: Range(1, 260)]
    int TargetTimelineWeeks,
    [property: Required, RegularExpression("dev|qa|ba|pm|support|other")]
    string Role);
