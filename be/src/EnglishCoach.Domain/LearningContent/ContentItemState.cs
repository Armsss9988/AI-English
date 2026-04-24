using System.Collections.Immutable;

namespace EnglishCoach.Domain.LearningContent;

public enum ContentState
{
    Draft,
    Review,
    Published,
    Deprecated,
    Archived
}

public static class ContentStateTransitions
{
    private static readonly Dictionary<ContentState, IImmutableSet<ContentState>> _allowedTransitions = new()
    {
        [ContentState.Draft] = ImmutableHashSet.Create(ContentState.Review),
        [ContentState.Review] = ImmutableHashSet.Create(ContentState.Draft, ContentState.Published),
        [ContentState.Published] = ImmutableHashSet.Create(ContentState.Deprecated),
        [ContentState.Deprecated] = ImmutableHashSet.Create(ContentState.Archived),
        [ContentState.Archived] = ImmutableHashSet<ContentState>.Empty,
    };

    public static bool CanTransition(ContentState from, ContentState to) =>
        _allowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    public static IReadOnlySet<ContentState> GetAllowedTransitions(ContentState from) =>
        _allowedTransitions.TryGetValue(from, out var allowed)
            ? allowed
            : ImmutableHashSet<ContentState>.Empty;
}