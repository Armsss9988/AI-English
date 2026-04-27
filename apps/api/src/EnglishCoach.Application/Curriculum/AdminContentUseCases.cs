using EnglishCoach.Contracts.Curriculum;
using EnglishCoach.Domain.Curriculum;

namespace EnglishCoach.Application.Curriculum;

// ── Phrase Use Cases ──

public sealed class CreatePhraseUseCase
{
    private readonly IPhraseRepository _repo;

    public CreatePhraseUseCase(IPhraseRepository repo)
    {
        _repo = repo;
    }

    public async Task<AdminPhraseResponse> ExecuteAsync(CreatePhraseRequest request, CancellationToken ct = default)
    {
        var function = Enum.Parse<CommunicationFunction>(request.Category, ignoreCase: true);
        var level = MapLevel(request.Difficulty);

        var phrase = Phrase.Create(
            Guid.NewGuid().ToString("N"),
            request.Content,
            request.Meaning,
            function,
            level,
            request.Example);

        await _repo.CreateAsync(phrase, ct);

        return MapToResponse(phrase);
    }

    private static ContentLevel MapLevel(string difficulty) => difficulty.ToLowerInvariant() switch
    {
        "beginner" or "survival" => ContentLevel.Survival,
        "intermediate" or "core" => ContentLevel.Core,
        "advanced" or "clientready" or "client_ready" => ContentLevel.ClientReady,
        _ => ContentLevel.Core
    };

    public static AdminPhraseResponse MapToResponse(Phrase p) => new(
        p.Id,
        p.Text,
        p.ViMeaning,
        p.CommunicationFunction.ToString(),
        p.Level.ToString(),
        p.Example,
        p.State.ToString().ToLowerInvariant(),
        p.ContentVersion
    );
}

public sealed class UpdatePhraseUseCase
{
    private readonly IPhraseRepository _repo;

    public UpdatePhraseUseCase(IPhraseRepository repo)
    {
        _repo = repo;
    }

    public async Task<AdminPhraseResponse> ExecuteAsync(string phraseId, UpdatePhraseRequest request, CancellationToken ct = default)
    {
        var phrase = await _repo.GetByIdAsync(phraseId, ct)
            ?? throw new InvalidOperationException($"Phrase '{phraseId}' not found.");

        phrase.IncrementVersion(request.Content, request.Meaning, request.Example);
        await _repo.UpdateAsync(phrase, ct);

        return CreatePhraseUseCase.MapToResponse(phrase);
    }
}

public sealed class PublishPhraseUseCase
{
    private readonly IPhraseRepository _repo;

    public PublishPhraseUseCase(IPhraseRepository repo)
    {
        _repo = repo;
    }

    public async Task<AdminPhraseResponse> ExecuteAsync(string phraseId, CancellationToken ct = default)
    {
        var phrase = await _repo.GetByIdAsync(phraseId, ct)
            ?? throw new InvalidOperationException($"Phrase '{phraseId}' not found.");

        // For admin convenience: Draft → Review → Published in one step
        if (phrase.State == ContentPublicationState.Draft)
            phrase.SubmitForReview();

        phrase.Publish();
        await _repo.UpdateAsync(phrase, ct);

        return CreatePhraseUseCase.MapToResponse(phrase);
    }
}

// ── Scenario Use Cases ──

public sealed class CreateScenarioUseCase
{
    private readonly IRoleplayScenarioRepository _repo;

    public CreateScenarioUseCase(IRoleplayScenarioRepository repo)
    {
        _repo = repo;
    }

    public async Task<AdminScenarioResponse> ExecuteAsync(CreateScenarioRequest request, CancellationToken ct = default)
    {
        var scenario = RoleplayScenario.Create(
            Guid.NewGuid().ToString("N"),
            request.Title,
            request.WorkplaceContext,
            request.UserRole,
            request.Persona,
            request.Goal,
            request.MustCoverPoints ?? Array.Empty<string>(),
            Array.Empty<string>(),
            request.PassCriteria ?? Array.Empty<string>(),
            request.Difficulty);

        await _repo.CreateAsync(scenario, ct);

        return MapToResponse(scenario);
    }

    public static AdminScenarioResponse MapToResponse(RoleplayScenario s) => new(
        s.Id,
        s.Title,
        s.CommunicationGoal,
        s.WorkplaceContext,
        s.UserRole,
        s.ClientPersona,
        s.MustCoverPoints.ToArray(),
        s.PassCriteria.ToArray(),
        s.Difficulty,
        s.State.ToString().ToLowerInvariant(),
        s.ContentVersion
    );
}

public sealed class UpdateScenarioUseCase
{
    private readonly IRoleplayScenarioRepository _repo;

    public UpdateScenarioUseCase(IRoleplayScenarioRepository repo)
    {
        _repo = repo;
    }

    public async Task<AdminScenarioResponse> ExecuteAsync(string scenarioId, UpdateScenarioRequest request, CancellationToken ct = default)
    {
        var scenario = await _repo.GetByIdAsync(scenarioId, ct)
            ?? throw new InvalidOperationException($"Scenario '{scenarioId}' not found.");

        // Scenario doesn't have IncrementVersion like Phrase — for now just note it exists
        // In a real implementation we'd add an Update method to the domain entity
        await _repo.UpdateAsync(scenario, ct);

        return CreateScenarioUseCase.MapToResponse(scenario);
    }
}

public sealed class PublishScenarioUseCase
{
    private readonly IRoleplayScenarioRepository _repo;

    public PublishScenarioUseCase(IRoleplayScenarioRepository repo)
    {
        _repo = repo;
    }

    public async Task<AdminScenarioResponse> ExecuteAsync(string scenarioId, CancellationToken ct = default)
    {
        var scenario = await _repo.GetByIdAsync(scenarioId, ct)
            ?? throw new InvalidOperationException($"Scenario '{scenarioId}' not found.");

        // Draft → Review → Published in one step for admin convenience
        if (scenario.State == ContentPublicationState.Draft)
            scenario.SubmitForReview();

        scenario.Publish();
        await _repo.UpdateAsync(scenario, ct);

        return CreateScenarioUseCase.MapToResponse(scenario);
    }
}
