namespace EnglishCoach.Domain.Roleplay;

public enum RoleplaySessionState
{
    Created,
    Active,
    AwaitingFeedback,
    Finalized,
    Archived
}

public static class RoleplaySessionStateTransitions
{
    public static IReadOnlySet<RoleplaySessionState> GetAllowedTransitions(RoleplaySessionState from)
    {
        return from switch
        {
            RoleplaySessionState.Created => new HashSet<RoleplaySessionState> { RoleplaySessionState.Active, RoleplaySessionState.Archived },
            RoleplaySessionState.Active => new HashSet<RoleplaySessionState> { RoleplaySessionState.AwaitingFeedback, RoleplaySessionState.Archived },
            RoleplaySessionState.AwaitingFeedback => new HashSet<RoleplaySessionState> { RoleplaySessionState.Finalized, RoleplaySessionState.Archived },
            RoleplaySessionState.Finalized => new HashSet<RoleplaySessionState> { RoleplaySessionState.Archived },
            RoleplaySessionState.Archived => new HashSet<RoleplaySessionState>(),
            _ => new HashSet<RoleplaySessionState>()
        };
    }
}

public enum TurnRole
{
    Learner,
    Client
}

public sealed class RoleplayTurn
{
    private RoleplayTurn()
    {
        Id = string.Empty;
        SessionId = string.Empty;
        Message = string.Empty;
        AudioUrl = string.Empty;
    }

    public string Id { get; private set; }
    public string SessionId { get; private set; }
    public TurnRole Role { get; private set; }
    public string Message { get; private set; }
    public string AudioUrl { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    internal static RoleplayTurn Create(string sessionId, TurnRole role, string message, string audioUrl = "")
    {
        return new RoleplayTurn
        {
            Id = Guid.NewGuid().ToString("N"),
            SessionId = sessionId,
            Role = role,
            Message = message,
            AudioUrl = audioUrl,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }
}
