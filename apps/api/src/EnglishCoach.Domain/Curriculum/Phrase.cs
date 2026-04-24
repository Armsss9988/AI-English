namespace EnglishCoach.Domain.Curriculum;

/// <summary>
/// C1: Phrase content model — versioned learning material for IT English communication.
/// Content tables contain no learner progress.
/// </summary>
public sealed class Phrase
{
    private Phrase()
    {
        Id = string.Empty;
        Text = string.Empty;
        ViMeaning = string.Empty;
        Example = string.Empty;
    }

    public string Id { get; private set; }
    public string Text { get; private set; }
    public string ViMeaning { get; private set; }
    public CommunicationFunction CommunicationFunction { get; private set; }
    public ContentLevel Level { get; private set; }
    public string Example { get; private set; }
    public int ContentVersion { get; private set; }
    public ContentPublicationState State { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Phrase Create(
        string id,
        string text,
        string viMeaning,
        CommunicationFunction communicationFunction,
        ContentLevel level,
        string example)
    {
        return new Phrase
        {
            Id = RequireNonEmpty(id, nameof(id)),
            Text = RequireNonEmpty(text, nameof(text)),
            ViMeaning = RequireNonEmpty(viMeaning, nameof(viMeaning)),
            CommunicationFunction = communicationFunction,
            Level = level,
            Example = RequireNonEmpty(example, nameof(example)),
            ContentVersion = 1,
            State = ContentPublicationState.Draft,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
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

    public void IncrementVersion(string newText, string newViMeaning, string newExample)
    {
        if (State == ContentPublicationState.Archived)
            throw new InvalidOperationException("Archived content cannot be modified.");
        Text = RequireNonEmpty(newText, nameof(newText));
        ViMeaning = RequireNonEmpty(newViMeaning, nameof(newViMeaning));
        Example = RequireNonEmpty(newExample, nameof(newExample));
        ContentVersion++;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public bool IsPublished => State == ContentPublicationState.Published;

    private static string RequireNonEmpty(string value, string paramName) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", paramName) : value.Trim();
}

public enum CommunicationFunction
{
    Standup,
    Issue,
    Clarification,
    Eta,
    Recommendation,
    Summary
}

public enum ContentLevel
{
    Survival,
    Core,
    ClientReady
}

public enum ContentPublicationState
{
    Draft,
    Review,
    Published,
    Deprecated,
    Archived
}
