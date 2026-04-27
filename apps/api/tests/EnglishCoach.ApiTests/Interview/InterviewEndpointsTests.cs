using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EnglishCoach.Application.Ports;
using EnglishCoach.Contracts.InterviewPractice;
using EnglishCoach.Domain.InterviewPractice;
using EnglishCoach.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnglishCoach.ApiTests.Interview;

public sealed class InterviewEndpointsTests
{
    private const string ValidExtractedCvText =
        "Backend developer with three years of ASP.NET Core, React, PostgreSQL, Docker, and Azure experience.";

    [Fact]
    public async Task UploadCv_Returns_BadGateway_When_AnalysisProviderFails()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();
        using var factory = CreateFactory(database, providerSucceeds: false, extractedText: ValidExtractedCvText);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-interview-1");

        var response = await client.PostAsJsonAsync(
            "/me/interview/upload-cv",
            new UploadCvRequest("Backend developer with ASP.NET Core experience."));

        var payload = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.StatusCode == HttpStatusCode.BadGateway,
            $"Expected BadGateway but got {response.StatusCode}. Payload: {payload}");
        Assert.Contains("Failed to analyze CV", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UploadCvFile_Returns_BadRequest_When_File_Is_Missing()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();
        using var factory = CreateFactory(database, providerSucceeds: true, extractedText: ValidExtractedCvText);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-interview-file-1");

        using var content = new MultipartFormDataContent();

        var response = await client.PostAsync("/me/interview/upload-cv-file", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("CV PDF file is required", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task UploadCvFile_Returns_BadRequest_When_File_Is_Not_Pdf()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();
        using var factory = CreateFactory(database, providerSucceeds: true, extractedText: ValidExtractedCvText);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-interview-file-2");

        using var content = new MultipartFormDataContent();
        using var file = new ByteArrayContent("plain text"u8.ToArray());
        file.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(file, "file", "cv.txt");

        var response = await client.PostAsync("/me/interview/upload-cv-file", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Only PDF CV files are supported", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task UploadCvFile_Returns_BadRequest_When_Extracted_Text_Is_Empty()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();
        using var factory = CreateFactory(database, providerSucceeds: true, extractedText: "   ");
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-interview-file-3");

        using var content = CreatePdfUploadContent("fake pdf bytes"u8.ToArray());

        var response = await client.PostAsync("/me/interview/upload-cv-file", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Could not extract readable text", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task UploadCvFile_Returns_BadGateway_When_AnalysisProviderFails()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();
        using var factory = CreateFactory(database, providerSucceeds: false, extractedText: ValidExtractedCvText);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-interview-file-4");

        using var content = CreatePdfUploadContent("fake pdf bytes"u8.ToArray());

        var response = await client.PostAsync("/me/interview/upload-cv-file", content);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Contains("Failed to analyze CV", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task UploadCvFile_Returns_Ok_When_Pdf_Text_Is_Extracted_And_Analyzed()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();
        using var factory = CreateFactory(database, providerSucceeds: true, extractedText: ValidExtractedCvText);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-interview-file-5");

        using var content = CreatePdfUploadContent("fake pdf bytes"u8.ToArray());

        var response = await client.PostAsync("/me/interview/upload-cv-file", content);

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<UploadCvResponse>();
        Assert.NotNull(payload);
        Assert.Contains("Test Learner", payload!.CvAnalysis);
    }

    [Fact]
    public async Task GetLatestCvProfile_Returns_Latest_Analyzed_Profile()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();
        using var factory = CreateFactory(database, providerSucceeds: true, extractedText: ValidExtractedCvText);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-interview-latest-1");
        var profileId = await CreateAnalyzedProfileAsync(client);

        var response = await client.GetAsync("/me/interview/profile/latest");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<UploadCvResponse>();
        Assert.NotNull(payload);
        Assert.Equal(profileId, payload!.ProfileId);
        Assert.Contains("Test Learner", payload.CvAnalysis);
    }

    [Fact]
    public async Task StartInterview_Uses_Fallback_When_Jd_Analysis_Provider_Fails()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();
        using var factory = CreateFactory(
            database,
            providerSucceeds: true,
            extractedText: ValidExtractedCvText,
            jdSucceeds: false);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-interview-start-1");
        var profileId = await CreateAnalyzedProfileAsync(client);

        var response = await client.PostAsJsonAsync(
            "/me/interview/sessions",
            new StartInterviewRequest(profileId, "Senior Backend Engineer role with client communication.", "Mixed"));

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<StartInterviewResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Opening", payload!.QuestionCategory);
        Assert.Contains("background", payload.FirstQuestion, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(5, payload.PlannedQuestionCount);
    }

    [Fact]
    public async Task StartInterview_Uses_Local_Fake_Providers_When_OpenAi_ApiKey_Is_Empty()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();
        using var factory = CreateFactoryWithoutProviderOverrides(database);
        var client = factory.CreateClient();
        const string learnerId = "user-interview-empty-key-1";
        client.DefaultRequestHeaders.Add("X-User-Id", learnerId);
        var profileId = await SeedAnalyzedProfileAsync(factory, learnerId);

        var response = await client.PostAsJsonAsync(
            "/me/interview/sessions",
            new StartInterviewRequest(profileId, "Backend role with ASP.NET Core and React.", "Mixed"));

        var payloadText = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.IsSuccessStatusCode,
            $"Expected success when OpenAI:ApiKey is empty, got {response.StatusCode}. Payload: {payloadText}");
        var payload = await response.Content.ReadFromJsonAsync<StartInterviewResponse>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.FirstQuestion));
        Assert.Equal("Opening", payload.QuestionCategory);
    }

    [Fact]
    public async Task StartInterview_Uses_Fallback_When_Interview_Plan_Provider_Fails()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();
        using var factory = CreateFactory(
            database,
            providerSucceeds: true,
            extractedText: ValidExtractedCvText,
            jdSucceeds: true,
            planSucceeds: false);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-interview-start-2");
        var profileId = await CreateAnalyzedProfileAsync(client);

        var response = await client.PostAsJsonAsync(
            "/me/interview/sessions",
            new StartInterviewRequest(profileId, "Senior Backend Engineer role with client communication.", "Mixed"));

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<StartInterviewResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Opening", payload!.QuestionCategory);
        Assert.Contains("background", payload.FirstQuestion, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(5, payload.PlannedQuestionCount);
    }

    [Fact]
    public async Task StartInterview_Uses_Fallback_First_Question_When_First_Question_Provider_Fails()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();
        using var factory = CreateFactory(
            database,
            providerSucceeds: true,
            extractedText: ValidExtractedCvText,
            jdSucceeds: true,
            planSucceeds: true,
            conductorSucceeds: false);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-interview-start-3");
        var profileId = await CreateAnalyzedProfileAsync(client);

        var response = await client.PostAsJsonAsync(
            "/me/interview/sessions",
            new StartInterviewRequest(profileId, "Senior Backend Engineer role with client communication.", "Mixed"));

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<StartInterviewResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Opening", payload!.QuestionCategory);
        Assert.Contains("background", payload.FirstQuestion, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AnswerQuestion_Uses_Fallback_Next_Question_When_Provider_Fails()
    {
        await using var database = new SqliteConnection("Data Source=:memory:");
        await database.OpenAsync();
        using var factory = CreateFactory(
            database,
            providerSucceeds: true,
            extractedText: ValidExtractedCvText,
            jdSucceeds: true,
            planSucceeds: true,
            conductorSucceeds: false);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-interview-answer-1");
        var profileId = await CreateAnalyzedProfileAsync(client);
        var startResponse = await client.PostAsJsonAsync(
            "/me/interview/sessions",
            new StartInterviewRequest(profileId, "Senior Backend Engineer role with client communication.", "Mixed"));
        startResponse.EnsureSuccessStatusCode();
        var session = await startResponse.Content.ReadFromJsonAsync<StartInterviewResponse>();
        Assert.NotNull(session);

        var response = await client.PostAsJsonAsync(
            $"/me/interview/sessions/{session!.SessionId}/answer",
            new AnswerQuestionRequest("I built backend services with ASP.NET Core and PostgreSQL.", null));

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AnswerQuestionResponse>();
        Assert.NotNull(payload);
        Assert.Equal("FollowUp", payload!.QuestionCategory);
        Assert.Contains("example", payload.NextQuestion, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, payload.AnsweredCount);
    }

    private static WebApplicationFactory<Program> CreateFactory(
        SqliteConnection database,
        bool providerSucceeds,
        string extractedText,
        bool jdSucceeds = false,
        bool planSucceeds = false,
        bool conductorSucceeds = true)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(DbContextOptions<EnglishCoachDbContext>));
                    services.AddDbContext<EnglishCoachDbContext>(options => options.UseSqlite(database));

                    services.RemoveAll<IInterviewAnalysisService>();
                    services.AddScoped<IInterviewAnalysisService>(_ =>
                        new ConfigurableInterviewAnalysisService(providerSucceeds, jdSucceeds, planSucceeds));

                    services.RemoveAll<IInterviewConductorService>();
                    services.AddScoped<IInterviewConductorService>(_ =>
                        new ConfigurableInterviewConductorService(conductorSucceeds));

                    services.RemoveAll<ICvTextExtractor>();
                    services.AddScoped<ICvTextExtractor>(_ => new FixedCvTextExtractor(extractedText));

                    using var scope = services.BuildServiceProvider().CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<EnglishCoachDbContext>();
                    dbContext.Database.EnsureCreated();
                });
            });
    }

    private static WebApplicationFactory<Program> CreateFactoryWithoutProviderOverrides(
        SqliteConnection database)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["OpenAI:ApiKey"] = string.Empty
                    });
                });

                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(DbContextOptions<EnglishCoachDbContext>));
                    services.AddDbContext<EnglishCoachDbContext>(options => options.UseSqlite(database));

                    services.RemoveAll<ICvTextExtractor>();
                    services.AddScoped<ICvTextExtractor>(_ => new FixedCvTextExtractor(ValidExtractedCvText));

                    using var scope = services.BuildServiceProvider().CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<EnglishCoachDbContext>();
                    dbContext.Database.EnsureCreated();
                });
            });
    }

    private static async Task<Guid> SeedAnalyzedProfileAsync(
        WebApplicationFactory<Program> factory,
        string learnerId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EnglishCoachDbContext>();
        var profileId = Guid.NewGuid();
        var profile = InterviewProfile.Create(
            profileId.ToString(),
            learnerId,
            ValidExtractedCvText);
        profile.SetCvAnalysis("""{"name":"Seeded Learner","skills":["ASP.NET Core","React"]}""");
        await dbContext.InterviewProfiles.AddAsync(profile);
        await dbContext.SaveChangesAsync();
        return profileId;
    }

    private static async Task<Guid> CreateAnalyzedProfileAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/me/interview/upload-cv",
            new UploadCvRequest(ValidExtractedCvText));
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<UploadCvResponse>();
        Assert.NotNull(payload);
        return payload!.ProfileId;
    }

    private static MultipartFormDataContent CreatePdfUploadContent(byte[] bytes)
    {
        var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(file, "file", "cv.pdf");
        return content;
    }

    private sealed class ConfigurableInterviewAnalysisService : IInterviewAnalysisService
    {
        private readonly bool _succeed;
        private readonly bool _jdSucceeds;
        private readonly bool _planSucceeds;

        public ConfigurableInterviewAnalysisService(bool succeed, bool jdSucceeds, bool planSucceeds)
        {
            _succeed = succeed;
            _jdSucceeds = jdSucceeds;
            _planSucceeds = planSucceeds;
        }

        public ProviderKind Provider => ProviderKind.Fake;

        public Task<CvAnalysisResult> AnalyzeCvAsync(string cvText, CancellationToken ct = default)
        {
            return Task.FromResult(_succeed
                ? CvAnalysisResult.Success("""{"name":"Test Learner","yearsOfExperience":3}""", Provider)
                : CvAnalysisResult.Failure("NIM provider unavailable", Provider));
        }

        public Task<JdAnalysisResult> AnalyzeJdAsync(string jdText, string cvAnalysis, CancellationToken ct = default)
        {
            return Task.FromResult(_jdSucceeds
                ? JdAnalysisResult.Success("""{"matchScore":82}""", Provider)
                : JdAnalysisResult.Failure("NIM JD provider unavailable", Provider));
        }

        public Task<InterviewPlanResult> CreateInterviewPlanAsync(
            string cvAnalysis,
            string jdAnalysis,
            InterviewType interviewType,
            CancellationToken ct = default)
        {
            return Task.FromResult(_planSucceeds
                ? InterviewPlanResult.Success("""{"totalQuestions":5}""", 5, Provider)
                : InterviewPlanResult.Failure("NIM plan provider unavailable", Provider));
        }
    }

    private sealed class ConfigurableInterviewConductorService : IInterviewConductorService
    {
        private readonly bool _succeed;

        public ConfigurableInterviewConductorService(bool succeed)
        {
            _succeed = succeed;
        }

        public ProviderKind Provider => ProviderKind.Fake;

        public Task<InterviewQuestionResult> GenerateNextQuestionAsync(
            InterviewConductorContext context,
            CancellationToken ct = default)
        {
            return Task.FromResult(_succeed
                ? InterviewQuestionResult.Success(
                    new InterviewQuestionContent
                    {
                        Question = "Tell me about your backend experience.",
                        Category = "Opening",
                        CoachingHint = "Keep your answer concise.",
                        IsLastQuestion = false
                    },
                    Provider)
                : InterviewQuestionResult.Failure("NIM question provider unavailable", Provider));
        }

        public Task<InterviewFeedbackResult> EvaluateSessionAsync(
            InterviewConductorContext context,
            CancellationToken ct = default)
        {
            return Task.FromResult(InterviewFeedbackResult.Failure("Not used", Provider));
        }
    }

    private sealed class FixedCvTextExtractor : ICvTextExtractor
    {
        private readonly string _text;

        public FixedCvTextExtractor(string text)
        {
            _text = text;
        }

        public Task<string> ExtractTextAsync(Stream fileStream, CancellationToken ct = default)
        {
            return Task.FromResult(_text);
        }
    }
}
