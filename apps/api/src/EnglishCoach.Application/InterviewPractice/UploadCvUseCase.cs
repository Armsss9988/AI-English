using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.InterviewPractice;

namespace EnglishCoach.Application.InterviewPractice;

public sealed class UploadCvUseCase
{
    private readonly IInterviewProfileRepository _profileRepository;
    private readonly IInterviewAnalysisService _analysisService;

    public UploadCvUseCase(
        IInterviewProfileRepository profileRepository,
        IInterviewAnalysisService analysisService)
    {
        _profileRepository = profileRepository;
        _analysisService = analysisService;
    }

    public async Task<EnglishCoach.Contracts.InterviewPractice.UploadCvResponse> ExecuteAsync(
        string learnerId,
        string cvText,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cvText))
            throw new ArgumentException("CV text is required.", nameof(cvText));

        // Check if learner already has a profile; update if so
        var existing = await _profileRepository.GetLatestByLearnerIdAsync(learnerId, ct);
        var normalizedCvText = NormalizeCvText(cvText);

        if (existing is not null &&
            existing.CvText == normalizedCvText &&
            !string.IsNullOrWhiteSpace(existing.CvAnalysis))
        {
            return new EnglishCoach.Contracts.InterviewPractice.UploadCvResponse(
                Guid.Parse(existing.Id),
                existing.CvAnalysis
            );
        }

        InterviewProfile profile;
        if (existing is not null)
        {
            existing.UpdateCv(normalizedCvText);
            profile = existing;
        }
        else
        {
            profile = InterviewProfile.Create(Guid.NewGuid().ToString(), learnerId, normalizedCvText);
        }

        // Analyze CV via AI
        var analysisResult = await _analysisService.AnalyzeCvAsync(profile.CvText, ct);
        if (!analysisResult.IsSuccess || analysisResult.Analysis is null)
        {
            throw new InvalidOperationException("Failed to analyze CV: " + analysisResult.ErrorMessage);
        }

        profile.SetCvAnalysis(analysisResult.Analysis);

        if (existing is not null)
            await _profileRepository.UpdateAsync(profile, ct);
        else
            await _profileRepository.CreateAsync(profile, ct);

        return new EnglishCoach.Contracts.InterviewPractice.UploadCvResponse(
            Guid.Parse(profile.Id),
            profile.CvAnalysis
        );
    }

    private static string NormalizeCvText(string cvText)
    {
        return cvText.Replace("\0", string.Empty, StringComparison.Ordinal).Trim();
    }
}
