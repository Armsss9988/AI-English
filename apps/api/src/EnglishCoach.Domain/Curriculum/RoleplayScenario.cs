namespace EnglishCoach.Domain.Curriculum;

/// <summary>
/// C2: Roleplay scenario with explicit success criteria.
/// References target phrases, stores pass criteria as data not prompt text.
/// </summary>
public sealed class RoleplayScenario
{
    private RoleplayScenario()
    {
        Id = string.Empty;
        Title = string.Empty;
        WorkplaceContext = string.Empty;
        UserRole = string.Empty;
        ClientPersona = string.Empty;
        CommunicationGoal = string.Empty;
    }

    public string Id { get; private set; }
    public string Title { get; private set; }
    public string WorkplaceContext { get; private set; }
    public string UserRole { get; private set; }
    public string ClientPersona { get; private set; }
    public string CommunicationGoal { get; private set; }

    private readonly List<string> _mustCoverPoints = new();
    public IReadOnlyList<string> MustCoverPoints => _mustCoverPoints.AsReadOnly();

    private readonly List<string> _targetPhraseIds = new();
    public IReadOnlyList<string> TargetPhraseIds => _targetPhraseIds.AsReadOnly();

    private readonly List<string> _passCriteria = new();
    public IReadOnlyList<string> PassCriteria => _passCriteria.AsReadOnly();

    public int Difficulty { get; private set; }
    public int ContentVersion { get; private set; }
    public ContentPublicationState State { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static RoleplayScenario Create(
        string id,
        string title,
        string workplaceContext,
        string userRole,
        string clientPersona,
        string communicationGoal,
        IEnumerable<string> mustCoverPoints,
        IEnumerable<string> targetPhraseIds,
        IEnumerable<string> passCriteria,
        int difficulty)
    {
        var scenario = new RoleplayScenario
        {
            Id = RequireNonEmpty(id, nameof(id)),
            Title = RequireNonEmpty(title, nameof(title)),
            WorkplaceContext = RequireNonEmpty(workplaceContext, nameof(workplaceContext)),
            UserRole = RequireNonEmpty(userRole, nameof(userRole)),
            ClientPersona = RequireNonEmpty(clientPersona, nameof(clientPersona)),
            CommunicationGoal = RequireNonEmpty(communicationGoal, nameof(communicationGoal)),
            Difficulty = difficulty,
            ContentVersion = 1,
            State = ContentPublicationState.Draft,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        scenario._mustCoverPoints.AddRange(mustCoverPoints);
        scenario._targetPhraseIds.AddRange(targetPhraseIds);
        scenario._passCriteria.AddRange(passCriteria);

        return scenario;
    }

    public void Publish()
    {
        if (State != ContentPublicationState.Review)
            throw new InvalidOperationException($"Can only publish from Review state. Current: {State}");
        State = ContentPublicationState.Published;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SubmitForReview()
    {
        if (State != ContentPublicationState.Draft)
            throw new InvalidOperationException($"Can only submit from Draft state. Current: {State}");
        State = ContentPublicationState.Review;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Deprecate()
    {
        if (State != ContentPublicationState.Published)
            throw new InvalidOperationException($"Can only deprecate from Published state. Current: {State}");
        State = ContentPublicationState.Deprecated;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        if (State != ContentPublicationState.Deprecated)
            throw new InvalidOperationException($"Can only archive from Deprecated state. Current: {State}");
        State = ContentPublicationState.Archived;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public bool IsPublished => State == ContentPublicationState.Published;

    private static string RequireNonEmpty(string value, string paramName) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", paramName) : value.Trim();
}
