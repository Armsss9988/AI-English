using EnglishCoach.Domain.Entities;
using EnglishCoach.SharedKernel.Ids;

namespace EnglishCoach.Domain.LearningContent;

public class ContentItem : Entity
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public ContentType ContentType { get; private set; }
    public ContentState State { get; private set; }
    public int CurrentVersion { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation
    private readonly List<ContentVersion> _versions = new();
    public IReadOnlyList<ContentVersion> Versions => _versions.AsReadOnly();

    private ContentItem() { } // EF Core

    public ContentItem(Guid id, string title, ContentType contentType)
    {
        Id = id;
        Title = title;
        ContentType = contentType;
        State = ContentState.Draft;
        CurrentVersion = 1;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static ContentItem CreatePhrase(string title, string category, string usageExample)
    {
        var item = new ContentItem(Guid.NewGuid(), title, ContentType.Phrase);
        item._versions.Add(ContentVersion.CreateInitial(item.Id, 1, title, category, usageExample));
        return item;
    }

    public static ContentItem CreateScenario(string title, string description, string goal, string persona)
    {
        var item = new ContentItem(Guid.NewGuid(), title, ContentType.Scenario);
        item._versions.Add(ContentVersion.CreateInitial(item.Id, 1, title, description, goal, persona));
        return item;
    }

    public void SubmitForReview()
    {
        TransitionTo(ContentState.Review);
    }

    public void ReturnToDraft()
    {
        if (State != ContentState.Review)
            throw new InvalidOperationException($"Can only return from Review state. Current: {State}");

        if (!ContentStateTransitions.CanTransition(State, ContentState.Draft))
            throw new InvalidOperationException($"Transition from {State} to Draft is not allowed.");

        State = ContentState.Draft;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Publish()
    {
        if (State != ContentState.Review)
            throw new InvalidOperationException($"Can only publish from Review state. Current: {State}");

        if (!ContentStateTransitions.CanTransition(State, ContentState.Published))
            throw new InvalidOperationException($"Transition from {State} to Published is not allowed.");

        State = ContentState.Published;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deprecate()
    {
        if (State != ContentState.Published)
            throw new InvalidOperationException($"Can only deprecate from Published state. Current: {State}");

        if (!ContentStateTransitions.CanTransition(State, ContentState.Deprecated))
            throw new InvalidOperationException($"Transition from {State} to Deprecated is not allowed.");

        State = ContentState.Deprecated;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        if (State != ContentState.Deprecated)
            throw new InvalidOperationException($"Can only archive from Deprecated state. Current: {State}");

        if (!ContentStateTransitions.CanTransition(State, ContentState.Archived))
            throw new InvalidOperationException($"Transition from {State} to Archived is not allowed.");

        State = ContentState.Archived;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public ContentVersion CreateNewVersion(string content, string? category = null, string? usageExample = null)
    {
        if (State == ContentState.Published || State == ContentState.Deprecated)
        {
            throw new InvalidOperationException(
                "Cannot edit in place. Published content requires creating a new version via CreateNewPublishedVersion.");
        }

        if (State == ContentState.Archived)
        {
            throw new InvalidOperationException("Archived content cannot be modified.");
        }

        CurrentVersion++;
        var version = ContentVersion.Create(
            Id,
            CurrentVersion,
            Title,
            content,
            category,
            usageExample);

        _versions.Add(version);
        UpdatedAt = DateTimeOffset.UtcNow;
        return version;
    }

    public ContentVersion CreateNewPublishedVersion(string title, string content, string? category = null, string? usageExample = null)
    {
        // Published content: archive current, create new item with new version
        if (State != ContentState.Published && State != ContentState.Deprecated)
        {
            throw new InvalidOperationException(
                "Can only create new published version from Published or Deprecated state.");
        }

        CurrentVersion++;
        var version = ContentVersion.Create(
            Id,
            CurrentVersion,
            title,
            content,
            category,
            usageExample);

        _versions.Add(version);
        UpdatedAt = DateTimeOffset.UtcNow;
        return version;
    }

    public bool IsEditable => State == ContentState.Draft || State == ContentState.Review;
    public bool IsPublished => State == ContentState.Published;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }

    private void TransitionTo(ContentState newState)
    {
        if (!ContentStateTransitions.CanTransition(State, newState))
        {
            throw new InvalidOperationException(
                $"Invalid state transition from {State} to {newState}. Allowed: {string.Join(", ", ContentStateTransitions.GetAllowedTransitions(State))}");
        }

        State = newState;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public enum ContentType
{
    Phrase,
    Scenario
}