using System.ComponentModel.DataAnnotations;

namespace EnglishCoach.Contracts.Curriculum;

// ── Admin Create/Update DTOs ──

public sealed record CreatePhraseRequest(
    [Required] string Content,
    [Required] string Meaning,
    [Required] string Category,
    [Required] string Difficulty,
    [Required] string Example
);

public sealed record UpdatePhraseRequest(
    [Required] string Content,
    [Required] string Meaning,
    [Required] string Category,
    [Required] string Difficulty,
    [Required] string Example
);

public sealed record CreateScenarioRequest(
    [Required] string Title,
    [Required] string Goal,
    [Required] string WorkplaceContext,
    [Required] string UserRole,
    [Required] string Persona,
    string[] MustCoverPoints,
    string[] PassCriteria,
    int Difficulty
);

public sealed record UpdateScenarioRequest(
    [Required] string Title,
    [Required] string Goal,
    [Required] string WorkplaceContext,
    [Required] string UserRole,
    [Required] string Persona,
    string[] MustCoverPoints,
    string[] PassCriteria,
    int Difficulty
);

// ── Response DTOs ──

public sealed record AdminPhraseResponse(
    string Id,
    string Content,
    string Meaning,
    string Category,
    string Difficulty,
    string Example,
    string Status,
    int ContentVersion
);

public sealed record AdminScenarioResponse(
    string Id,
    string Title,
    string Goal,
    string WorkplaceContext,
    string UserRole,
    string Persona,
    string[] MustCoverPoints,
    string[] PassCriteria,
    int Difficulty,
    string Status,
    int ContentVersion
);
