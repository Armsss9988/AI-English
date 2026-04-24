namespace EnglishCoach.Domain.LearningContent;

public class ContentVersion
{
    public Guid Id { get; private set; }
    public Guid ContentItemId { get; private set; }
    public int VersionNumber { get; private set; }
    public string Title { get; private set; } = string.Empty;

    // Phrase-specific
    public string? Category { get; private set; }
    public string? UsageExample { get; private set; }

    // Scenario-specific
    public string? Description { get; private set; }
    public string? Goal { get; private set; }
    public string? Persona { get; private set; }

    // Metadata
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private ContentVersion() { } // EF Core

    public static ContentVersion CreateInitial(
        Guid contentItemId,
        int version,
        string title,
        string? category = null,
        string? usageExample = null)
    {
        return new ContentVersion
        {
            Id = Guid.NewGuid(),
            ContentItemId = contentItemId,
            VersionNumber = version,
            Title = title,
            Category = category,
            UsageExample = usageExample,
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public static ContentVersion CreateScenarioInitial(
        Guid contentItemId,
        int version,
        string title,
        string description,
        string goal,
        string persona)
    {
        return new ContentVersion
        {
            Id = Guid.NewGuid(),
            ContentItemId = contentItemId,
            VersionNumber = version,
            Title = title,
            Description = description,
            Goal = goal,
            Persona = persona,
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public static ContentVersion Create(
        Guid contentItemId,
        int version,
        string title,
        string? content = null,
        string? category = null,
        string? usageExample = null)
    {
        return new ContentVersion
        {
            Id = Guid.NewGuid(),
            ContentItemId = contentItemId,
            VersionNumber = version,
            Title = title,
            Category = category,
            UsageExample = usageExample,
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
