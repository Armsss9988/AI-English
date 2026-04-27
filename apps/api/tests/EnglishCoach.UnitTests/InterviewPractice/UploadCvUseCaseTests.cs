using EnglishCoach.Application.InterviewPractice;
using EnglishCoach.Application.Ports;
using EnglishCoach.Domain.InterviewPractice;
using Xunit;

namespace EnglishCoach.UnitTests.InterviewPractice;

public sealed class UploadCvUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_Removes_Null_Characters_Before_Analysis_And_Persistence()
    {
        var repository = new InMemoryInterviewProfileRepository();
        var analysisService = new RecordingInterviewAnalysisService("{\"name\":\"Nguyen\0 Van A\"}");
        var useCase = new UploadCvUseCase(repository, analysisService);

        var response = await useCase.ExecuteAsync("learner-1", "Backend\0 developer with ASP.NET Core");

        Assert.Equal("Backend developer with ASP.NET Core", analysisService.LastCvText);
        Assert.NotNull(repository.Profile);
        Assert.Equal("Backend developer with ASP.NET Core", repository.Profile!.CvText);
        Assert.Equal("""{"name":"Nguyen Van A"}""", repository.Profile.CvAnalysis);
        Assert.Equal("""{"name":"Nguyen Van A"}""", response.CvAnalysis);
    }

    [Fact]
    public async Task ExecuteAsync_Reuses_Existing_Analysis_When_Cv_Text_Is_Unchanged()
    {
        var repository = new InMemoryInterviewProfileRepository();
        var analysisService = new RecordingInterviewAnalysisService("""{"name":"Initial"}""");
        var useCase = new UploadCvUseCase(repository, analysisService);
        await useCase.ExecuteAsync("learner-1", "Backend developer with ASP.NET Core");
        analysisService.Reset();

        var response = await useCase.ExecuteAsync("learner-1", "  Backend developer with ASP.NET Core  ");

        Assert.Equal(0, analysisService.AnalyzeCvCallCount);
        Assert.NotNull(repository.Profile);
        Assert.Equal(repository.Profile!.Id, response.ProfileId.ToString());
        Assert.Equal("""{"name":"Initial"}""", response.CvAnalysis);
    }

    private sealed class InMemoryInterviewProfileRepository : IInterviewProfileRepository
    {
        public InterviewProfile? Profile { get; private set; }

        public Task<InterviewProfile?> GetByIdAsync(string profileId, CancellationToken ct = default)
        {
            return Task.FromResult(Profile?.Id == profileId ? Profile : null);
        }

        public Task<InterviewProfile?> GetLatestByLearnerIdAsync(string learnerId, CancellationToken ct = default)
        {
            return Task.FromResult(Profile?.LearnerId == learnerId ? Profile : null);
        }

        public Task CreateAsync(InterviewProfile profile, CancellationToken ct = default)
        {
            Profile = profile;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(InterviewProfile profile, CancellationToken ct = default)
        {
            Profile = profile;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingInterviewAnalysisService : IInterviewAnalysisService
    {
        private readonly string _analysis;

        public RecordingInterviewAnalysisService(string analysis)
        {
            _analysis = analysis;
        }

        public ProviderKind Provider => ProviderKind.Fake;
        public string? LastCvText { get; private set; }
        public int AnalyzeCvCallCount { get; private set; }

        public Task<CvAnalysisResult> AnalyzeCvAsync(string cvText, CancellationToken ct = default)
        {
            AnalyzeCvCallCount++;
            LastCvText = cvText;
            return Task.FromResult(CvAnalysisResult.Success(_analysis, Provider));
        }

        public void Reset()
        {
            AnalyzeCvCallCount = 0;
            LastCvText = null;
        }

        public Task<JdAnalysisResult> AnalyzeJdAsync(string jdText, string cvAnalysis, CancellationToken ct = default)
        {
            return Task.FromResult(JdAnalysisResult.Failure("Not used", Provider));
        }

        public Task<InterviewPlanResult> CreateInterviewPlanAsync(
            string cvAnalysis,
            string jdAnalysis,
            InterviewType interviewType,
            CancellationToken ct = default)
        {
            return Task.FromResult(InterviewPlanResult.Failure("Not used", Provider));
        }
    }
}
