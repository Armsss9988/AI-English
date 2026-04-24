using EnglishCoach.Application.Ports;

namespace EnglishCoach.Infrastructure.AI.FakeAdapters;

public class FakeRoleplayService : IRoleplayResponseService
{
    public ProviderKind Provider => ProviderKind.Fake;

    private readonly RoleplayResponseContent _response;
    private readonly bool _shouldFail;
    private readonly string _errorCode;
    private readonly string _errorMessage;

    public FakeRoleplayService(
        RoleplayResponseContent? response = null,
        bool shouldFail = false,
        string errorCode = "ROLEPLAY_GENERATION_FAILED",
        string errorMessage = "Simulated roleplay failure")
    {
        _response = response ?? CreateDefaultResponse();
        _shouldFail = shouldFail;
        _errorCode = errorCode;
        _errorMessage = errorMessage;
    }

    public Task<RoleplayResult> GenerateResponseAsync(
        RoleplayContext context,
        CancellationToken ct = default)
    {
        if (_shouldFail)
        {
            return Task.FromResult(RoleplayResult.Failure(_errorCode, _errorMessage, Provider));
        }

        return Task.FromResult(RoleplayResult.Success(_response, Provider));
    }

    private static RoleplayResponseContent CreateDefaultResponse() => new()
    {
        ClientMessage = "Thank you for the update. Let me review the details and get back to you.",
        CoachingNote = "Good use of technical terminology.",
        IsSessionComplete = false,
        EvaluatedCriteria = new[] { "Clarity", "Technical accuracy" }
    };

    public static FakeRoleplayService Success(RoleplayResponseContent? response = null)
        => new(response);

    public static FakeRoleplayService Failure(string errorCode = "ROLEPLAY_GENERATION_FAILED", string errorMessage = "Simulated failure")
        => new(shouldFail: true, errorCode: errorCode, errorMessage: errorMessage);
}