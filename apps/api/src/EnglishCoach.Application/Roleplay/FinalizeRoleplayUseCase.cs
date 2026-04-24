using EnglishCoach.Application.Curriculum;
using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.Roleplay;

namespace EnglishCoach.Application.Roleplay;

public sealed class FinalizeRoleplayUseCase
{
    private readonly IRoleplaySessionRepository _sessionRepository;
    private readonly IRoleplayScenarioRepository _scenarioRepository;
    private readonly ISpeakingFeedbackService _feedbackService;

    public FinalizeRoleplayUseCase(
        IRoleplaySessionRepository sessionRepository,
        IRoleplayScenarioRepository scenarioRepository,
        ISpeakingFeedbackService feedbackService)
    {
        _sessionRepository = sessionRepository;
        _scenarioRepository = scenarioRepository;
        _feedbackService = feedbackService;
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

        // Extract learner turns
        var learnerTurns = session.Turns
            .Where(t => t.Role == TurnRole.Learner)
            .Select(t => t.Message)
            .ToList();

        // In a real app, this would use a specialized roleplay feedback service.
        // For MVP, we'll simulate a roleplay summary evaluation using fake service logic.
        // We map to RoleplaySummary properly.
        
        var summary = new RoleplaySummary(
            "Passed",
            "You addressed the client well.",
            "Watch out for tense agreement.",
            "I will check with the team and get back to you.",
            "check with the team",
            "Try to be more confident in the intro."
        );

        session.Finalize(summary);
        
        // Emits RoleplaySessionFinalized event (To be added via outbox or domain events in infrastructure)
        
        await _sessionRepository.UpdateAsync(session, ct);

        return summary;
    }
}
