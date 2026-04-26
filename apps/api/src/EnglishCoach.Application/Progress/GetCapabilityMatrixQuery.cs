using EnglishCoach.Contracts.Progress;
using EnglishCoach.Domain.Progress;

namespace EnglishCoach.Application.Progress;

public interface ILearnerProgressDataProvider
{
    Task<LearnerProgressData> GetLearnerProgressAsync(Guid learnerId, CancellationToken ct = default);
}

public class GetCapabilityMatrixQuery
{
    private readonly ILearnerProgressDataProvider _dataProvider;

    public GetCapabilityMatrixQuery(ILearnerProgressDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public async Task<CapabilityMatrixResponse> ExecuteAsync(Guid learnerId, CancellationToken ct = default)
    {
        var progressData = await _dataProvider.GetLearnerProgressAsync(learnerId, ct);
        var matrix = new CapabilityMatrix(new[] { progressData });
        
        var assessments = matrix.Evaluate();

        var capabilities = assessments.Select(a => new CapabilityResponse(
            a.Name.ToString(),
            a.Status.ToString(),
            a.Explanation,
            a.Evidence
        )).ToList();

        return new CapabilityMatrixResponse(capabilities);
    }
}
