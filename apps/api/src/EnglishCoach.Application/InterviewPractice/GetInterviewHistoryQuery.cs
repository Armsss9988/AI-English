using EnglishCoach.Domain.InterviewPractice;

namespace EnglishCoach.Application.InterviewPractice;

public sealed class GetInterviewHistoryQuery
{
    private readonly IInterviewSessionRepository _sessionRepository;

    public GetInterviewHistoryQuery(IInterviewSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<EnglishCoach.Contracts.InterviewPractice.InterviewHistoryResponse> ExecuteAsync(
        string learnerId,
        CancellationToken ct = default)
    {
        var sessions = await _sessionRepository.GetByLearnerIdAsync(learnerId, ct);

        var items = sessions
            .OrderByDescending(s => s.CreatedAtUtc)
            .Select(s => new EnglishCoach.Contracts.InterviewPractice.InterviewHistoryItem(
                Guid.Parse(s.Id),
                s.Type.ToString(),
                s.Mode.ToString(),
                s.State.ToString(),
                s.PlannedQuestionCount,
                s.LearnerAnswerCount,
                s.Feedback?.OverallScore,
                s.CreatedAtUtc
            ))
            .ToList();

        return new EnglishCoach.Contracts.InterviewPractice.InterviewHistoryResponse(items);
    }
}
