using EnglishCoach.Contracts.InterviewPractice;

namespace EnglishCoach.Application.InterviewPractice;

public sealed class GetLatestInterviewProfileQuery
{
    private readonly IInterviewProfileRepository _profileRepository;

    public GetLatestInterviewProfileQuery(IInterviewProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<UploadCvResponse?> ExecuteAsync(string learnerId, CancellationToken ct = default)
    {
        var profile = await _profileRepository.GetLatestByLearnerIdAsync(learnerId, ct);
        if (profile is null || string.IsNullOrWhiteSpace(profile.CvAnalysis))
        {
            return null;
        }

        return new UploadCvResponse(Guid.Parse(profile.Id), profile.CvAnalysis);
    }
}
