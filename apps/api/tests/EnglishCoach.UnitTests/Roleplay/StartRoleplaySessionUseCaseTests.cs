using EnglishCoach.Application.Curriculum;
using EnglishCoach.Application.Ports;
using EnglishCoach.Application.Roleplay;
using EnglishCoach.Domain.Curriculum;
using EnglishCoach.Domain.Roleplay;

namespace EnglishCoach.UnitTests.Roleplay;

public class StartRoleplaySessionUseCaseTests
{
    // ── RP3 Acceptance: Scenario is loaded from curriculum ──

    [Fact]
    public async Task Execute_LoadsScenarioAndCreatesSession()
    {
        var scenario = CreatePublishedScenario();
        var scenarioRepo = new FakeScenarioRepo(scenario);
        var sessionRepo = new FakeSessionRepo();
        var aiService = new FakeRoleplayService();
        var useCase = new StartRoleplaySessionUseCase(scenarioRepo, sessionRepo, aiService);

        var request = new EnglishCoach.Contracts.Roleplay.StartRoleplayRequest(scenario.Id);
        var result = await useCase.ExecuteAsync("learner-1", request);

        Assert.NotEqual(Guid.Empty, result.SessionId);
        Assert.NotEmpty(result.InitialMessage);
        Assert.Single(sessionRepo.Created);
    }

    // ── RP3 Acceptance: Session stores scenario content version ──

    [Fact]
    public async Task Execute_StoresScenarioContentVersion()
    {
        var scenario = CreatePublishedScenario();
        var sessionRepo = new FakeSessionRepo();
        var useCase = new StartRoleplaySessionUseCase(
            new FakeScenarioRepo(scenario), sessionRepo, new FakeRoleplayService());

        var request = new EnglishCoach.Contracts.Roleplay.StartRoleplayRequest(scenario.Id);
        await useCase.ExecuteAsync("learner-1", request);

        var session = sessionRepo.Created[0];
        Assert.Equal(1, session.ScenarioContentVersion);
    }

    // ── RP3 Acceptance: First message is generated through adapter boundary ──

    [Fact]
    public async Task Execute_FirstMessageComesFromAIService()
    {
        var scenario = CreatePublishedScenario();
        var useCase = new StartRoleplaySessionUseCase(
            new FakeScenarioRepo(scenario), new FakeSessionRepo(), new FakeRoleplayService());

        var request = new EnglishCoach.Contracts.Roleplay.StartRoleplayRequest(scenario.Id);
        var result = await useCase.ExecuteAsync("learner-1", request);

        Assert.Contains("Hello", result.InitialMessage);
    }

    // ── Unpublished scenario rejected ──

    [Fact]
    public async Task Execute_WithUnpublishedScenario_Throws()
    {
        var scenario = RoleplayScenario.Create(
            Guid.NewGuid().ToString(), "Draft Scenario", "ctx", "role", "persona", "goal",
            Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), 1);
        // Stays in Draft

        var useCase = new StartRoleplaySessionUseCase(
            new FakeScenarioRepo(scenario), new FakeSessionRepo(), new FakeRoleplayService());

        var request = new EnglishCoach.Contracts.Roleplay.StartRoleplayRequest(scenario.Id);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync("learner-1", request));
    }

    // ── Controller stays thin (verified structurally in Program.cs) ──

    // ── Helpers ──

    private static RoleplayScenario CreatePublishedScenario()
    {
        var s = RoleplayScenario.Create(
            Guid.NewGuid().ToString(), "Test Scenario", "context", "role", "persona", "goal",
            new[] { "Point 1" }, new[] { "phrase-1" }, new[] { "Be polite" }, 2);
        s.SubmitForReview();
        s.Publish();
        return s;
    }

    private sealed class FakeScenarioRepo : IRoleplayScenarioRepository
    {
        private readonly RoleplayScenario? _scenario;
        public FakeScenarioRepo(RoleplayScenario? scenario) => _scenario = scenario;

        public Task<RoleplayScenario?> GetByIdAsync(string scenarioId, CancellationToken ct = default)
            => Task.FromResult(_scenario?.Id == scenarioId ? _scenario : null);

        public Task<IReadOnlyList<RoleplayScenario>> GetAllPublishedAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<RoleplayScenario>>(
                _scenario is { IsPublished: true } ? new[] { _scenario } : Array.Empty<RoleplayScenario>());

        public Task CreateAsync(RoleplayScenario scenario, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateAsync(RoleplayScenario scenario, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeSessionRepo : IRoleplaySessionRepository
    {
        public List<RoleplaySession> Created { get; } = new();

        public Task<RoleplaySession?> GetByIdAsync(string sessionId, CancellationToken ct = default)
            => Task.FromResult(Created.FirstOrDefault(s => s.Id == sessionId));

        public Task CreateAsync(RoleplaySession session, CancellationToken ct = default)
        {
            Created.Add(session);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(RoleplaySession session, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class FakeRoleplayService : IRoleplayResponseService
    {
        public ProviderKind Provider => ProviderKind.Fake;

        public Task<RoleplayResult> GenerateResponseAsync(RoleplayContext context, CancellationToken ct = default)
        {
            return Task.FromResult(RoleplayResult.Success(new RoleplayResponseContent
            {
                ClientMessage = "Hello! I'd like to discuss the project timeline.",
                CoachingNote = "Good start",
                IsSessionComplete = false
            }, ProviderKind.Fake));
        }
    }
}
