using System.ComponentModel.DataAnnotations;

namespace EnglishCoach.Contracts.Identity;

public sealed record UpdateMyProfileRequest(
    [property: Required, RegularExpression("dev|qa|ba|pm|support|other")]
    string Role,
    [property: Required, RegularExpression("A1|A2|B1|B2|C1|C2")]
    string CurrentLevel,
    [property: Required, StringLength(64, MinimumLength = 1)]
    string Timezone,
    [property: Required, StringLength(240, MinimumLength = 1)]
    string TargetUseCase,
    [property: StringLength(120)]
    string? DisplayName = null,
    [property: StringLength(16)]
    string? NativeLanguage = null,
    [property: Range(1, 260)]
    int? TargetTimelineWeeks = null);
