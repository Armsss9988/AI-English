using EnglishCoach.Application.Curriculum;
using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.Roleplay;

namespace EnglishCoach.Application.Roleplay;

public sealed class RecordTurnUseCase
{
    private readonly IRoleplaySessionRepository _sessionRepository;
    private readonly IRoleplayScenarioRepository _scenarioRepository;
    private readonly IRoleplayResponseService _aiService;

    public RecordTurnUseCase(
        IRoleplaySessionRepository sessionRepository,
        IRoleplayScenarioRepository scenarioRepository,
        IRoleplayResponseService aiService)
    {
        _sessionRepository = sessionRepository;
        _scenarioRepository = scenarioRepository;
        _aiService = aiService;
    }

    public async Task<EnglishCoach.Contracts.Roleplay.RecordTurnResponse> ExecuteAsync(
        string learnerId,
        Guid sessionId,
        EnglishCoach.Contracts.Roleplay.RecordTurnRequest request,
        CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId.ToString(), ct);
        if (session is null || session.LearnerId != learnerId)
        {
            throw new InvalidOperationException("Session not found or access denied.");
        }

        var scenario = await _scenarioRepository.GetByIdAsync(session.ScenarioId, ct);
        if (scenario is null)
        {
            throw new InvalidOperationException("Scenario not found.");
        }

        // Add learner turn
        session.AddLearnerTurn(request.LearnerMessage);
        
        // Persist learner message before generating reply, as per acceptance criteria
        await _sessionRepository.UpdateAsync(session, ct);

        // Generate AI response
        var pastTurns = session.Turns.Select(t => new RoleplayTurnRecord
        {
            Speaker = t.Role.ToString(),
            Message = t.Message,
            Timestamp = t.CreatedAtUtc
        }).ToArray();

        var context = new RoleplayContext
        {
            SessionId = sessionId,
            ScenarioId = Guid.Parse(scenario.Id),
            ScenarioTitle = scenario.Title,
            ScenarioPersona = scenario.ClientPersona,
            ScenarioGoal = scenario.CommunicationGoal,
            Difficulty = scenario.Difficulty,
            SuccessCriteria = scenario.PassCriteria,
            ConversationHistory = pastTurns,
            LatestLearnerTurn = pastTurns.LastOrDefault(t => t.Speaker == TurnRole.Learner.ToString())
        };

        var aiResponse = await _aiService.GenerateResponseAsync(context, ct);
        if (!aiResponse.IsSuccess || aiResponse.Content is null)
        {
            throw new InvalidOperationException("Failed to generate AI response: " + aiResponse.ErrorMessage);
        }

        // Add AI turn
        session.AddClientTurn(aiResponse.Content.ClientMessage);
        await _sessionRepository.UpdateAsync(session, ct);

        return new EnglishCoach.Contracts.Roleplay.RecordTurnResponse(
            aiResponse.Content.ClientMessage,
            aiResponse.Content.CoachingNote,
            aiResponse.Content.IsSessionComplete
        );
    }
}
