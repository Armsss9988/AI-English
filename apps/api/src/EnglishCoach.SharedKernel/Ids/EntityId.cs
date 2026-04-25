namespace EnglishCoach.SharedKernel.Ids;

public abstract record EntityId(Guid Value)
{
    public override string ToString() => Value.ToString();

    public static bool TryParse<T>(string? input, out T? result) where T : EntityId
    {
        if (Guid.TryParse(input, out var guid))
        {
            result = Create<T>(guid);
            return true;
        }
        result = null;
        return false;
    }

    protected static T Create<T>(Guid guid) where T : EntityId
    {
        return (T)Activator.CreateInstance(typeof(T), guid)!;
    }
}

public sealed record UserId(Guid Value) : EntityId(Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public static UserId Create(Guid guid) => new(guid);
}

public sealed record PhraseId(Guid Value) : EntityId(Value)
{
    public static PhraseId New() => new(Guid.NewGuid());
    public static PhraseId Create(Guid guid) => new(guid);
}

public sealed record SessionId(Guid Value) : EntityId(Value)
{
    public static SessionId New() => new(Guid.NewGuid());
    public static SessionId Create(Guid guid) => new(guid);
}

public sealed record ScenarioId(Guid Value) : EntityId(Value)
{
    public static ScenarioId New() => new(Guid.NewGuid());
    public static ScenarioId Create(Guid guid) => new(guid);
}

public sealed record ReviewItemId(Guid Value) : EntityId(Value)
{
    public static ReviewItemId New() => new(Guid.NewGuid());
    public static ReviewItemId Create(Guid guid) => new(guid);
}

public sealed record NotebookEntryId(Guid Value) : EntityId(Value)
{
    public static NotebookEntryId New() => new(Guid.NewGuid());
    public static NotebookEntryId Create(Guid guid) => new(guid);
}

public sealed record ReadinessSnapshotId(Guid Value) : EntityId(Value)
{
    public static ReadinessSnapshotId New() => new(Guid.NewGuid());
    public static ReadinessSnapshotId Create(Guid guid) => new(guid);
}
