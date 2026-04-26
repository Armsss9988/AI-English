using EnglishCoach.Application.Curriculum;
using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.Roleplay;

namespace EnglishCoach.Application.Roleplay;

public sealed class FinalizeRoleplayUseCase
{
    private readonly IRoleplaySessionRepository _sessionRepository;
    private readonly IRoleplayScenarioRepository _scenarioRepository;
    private readonly IRoleplayResponseService _aiService;

    public FinalizeRoleplayUseCase(
        IRoleplaySessionRepository sessionRepository,
        IRoleplayScenarioRepository scenarioRepository,
        IRoleplayResponseService aiService)
    {
        _sessionRepository = sessionRepository;
        _scenarioRepository = scenarioRepository;
        _aiService = aiService;
    }

    public async Task<RoleplaySummary> ExecuteAsync(
        string learnerId,
        Guid sessionId,
        CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId.ToString(), ct);
        if (session is null || session.LearnerId != learnerId)
        {
            throw new InvalidOperationException("Session not found or access denied.");
        }

        session.RequestFeedback();
        await _sessionRepository.UpdateAsync(session, ct);

        var scenario = await _scenarioRepository.GetByIdAsync(session.ScenarioId, ct);
        if (scenario is null)
        {
            throw new InvalidOperationException("Scenario not found.");
        }

        var context = new RoleplayContext
        {
            SessionId = Guid.Parse(session.Id),
            ScenarioId = Guid.Parse(scenario.Id),
            ScenarioTitle = scenario.Title,
            ScenarioPersona = scenario.ClientPersona,
            ScenarioGoal = scenario.CommunicationGoal,
            ConversationHistory = session.Turns.Select(t => new RoleplayTurnRecord
            {
                Speaker = t.Role.ToString(),
                Message = t.Message,
                Timestamp = t.CreatedAtUtc
            }).ToList(),
            Difficulty = scenario.Difficulty,
            SuccessCriteria = scenario.PassCriteria
        };

        var summary = await _aiService.EvaluateSessionAsync(context, ct);

        session.Finalize(summary);
        
        // Emits RoleplaySessionFinalized event (To be added via outbox or domain events in infrastructure)
        
        await _sessionRepository.UpdateAsync(session, ct);

        return summary;
    }
}
