namespace EnglishCoach.Contracts.Identity;

/// <summary>
/// Response describing the current identity mode of the API.
/// In personal-local mode, no real auth is required.
/// </summary>
public sealed record PersonalModeIdentityResponse(
    string UserId,
    string Mode,
    bool IsAdmin,
    string Note
);
