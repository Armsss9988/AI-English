using EnglishCoach.Application.Curriculum;
using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.Roleplay;

namespace EnglishCoach.Application.Roleplay;

public sealed class StartRoleplaySessionUseCase
{
    private readonly IRoleplayScenarioRepository _scenarioRepository;
    private readonly IRoleplaySessionRepository _sessionRepository;
    private readonly IRoleplayResponseService _aiService;

    public StartRoleplaySessionUseCase(
        IRoleplayScenarioRepository scenarioRepository,
        IRoleplaySessionRepository sessionRepository,
        IRoleplayResponseService aiService)
    {
        _scenarioRepository = scenarioRepository;
        _sessionRepository = sessionRepository;
        _aiService = aiService;
    }

    public async Task<EnglishCoach.Contracts.Roleplay.StartRoleplayResponse> ExecuteAsync(
        string learnerId,
        EnglishCoach.Contracts.Roleplay.StartRoleplayRequest request,
        CancellationToken ct = default)
    {
        var scenario = await _scenarioRepository.GetByIdAsync(request.ScenarioId.ToString(), ct);
        if (scenario is null || !scenario.IsPublished)
        {
            throw new InvalidOperationException("Scenario not found or not published.");
        }

        var sessionId = Guid.NewGuid().ToString("N");
        var session = RoleplaySession.Create(
            sessionId,
            learnerId,
            scenario.Id,
            scenario.ContentVersion
        );

        // Generate first AI message
        var context = new RoleplayContext
        {
            SessionId = Guid.Parse(sessionId),
            ScenarioId = Guid.Parse(scenario.Id),
            ScenarioTitle = scenario.Title,
            ScenarioPersona = scenario.ClientPersona,
            ScenarioGoal = scenario.CommunicationGoal,
            Difficulty = scenario.Difficulty,
            SuccessCriteria = scenario.PassCriteria
        };

        var aiResponse = await _aiService.GenerateResponseAsync(context, ct);
        if (!aiResponse.IsSuccess || aiResponse.Content is null)
        {
            throw new InvalidOperationException("Failed to generate AI response: " + aiResponse.ErrorMessage);
        }

        session.AddClientTurn(aiResponse.Content.ClientMessage);
        
        await _sessionRepository.CreateAsync(session, ct);

        return new EnglishCoach.Contracts.Roleplay.StartRoleplayResponse(
            Guid.Parse(sessionId),
            session.State.ToString(),
            scenario.Title,
            aiResponse.Content.ClientMessage
        );
    }
}
