using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using EnglishCoach.Application.Identity;
using EnglishCoach.Application.Review;
using EnglishCoach.Contracts.Identity;
using EnglishCoach.Contracts.Review;
using EnglishCoach.Application.Curriculum;
using EnglishCoach.Application.ErrorNotebook;
using EnglishCoach.Application.Roleplay;
using EnglishCoach.Application.Speaking;
using EnglishCoach.Infrastructure.Identity;
using EnglishCoach.Infrastructure.Persistence;
using EnglishCoach.Infrastructure.Review;
using EnglishCoach.Infrastructure.Persistence.Repositories;
using EnglishCoach.Infrastructure.ErrorNotebook;
using EnglishCoach.SharedKernel.Time;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<EnglishCoachDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("EnglishCoach")
        ?? "Host=127.0.0.1;Port=9999;Database=englishcoach_dev;Username=postgres;Password=postgres";

    options.UseNpgsql(connectionString);
});
builder.Services.AddScoped<ILearnerProfileRepository, LearnerProfileRepository>();
builder.Services.AddScoped<GetMyProfileUseCase>();
builder.Services.AddScoped<UpdateMyProfileUseCase>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IPhraseRepository, PhraseRepository>();
builder.Services.AddScoped<IRoleplayScenarioRepository, RoleplayScenarioRepository>();
builder.Services.AddScoped<IRoleplaySessionRepository, RoleplaySessionRepository>();
builder.Services.AddScoped<INotebookRepository, NotebookRepository>();
builder.Services.AddScoped<IReviewIntegrationService, ReviewIntegrationService>();
builder.Services.AddScoped<PromoteErrorPatternUseCase>();
builder.Services.AddScoped<GetNotebookEntriesUseCase>();
builder.Services.AddScoped<StartRoleplaySessionUseCase>();
builder.Services.AddScoped<RecordTurnUseCase>();
builder.Services.AddScoped<FinalizeRoleplayUseCase>();
builder.Services.AddScoped<ISpeakingAttemptRepository, SpeakingAttemptRepository>();
builder.Services.AddScoped<CreateSpeakingAttemptUseCase>();
builder.Services.AddScoped<SubmitSpeakingAttemptEvaluationUseCase>();
builder.Services.AddScoped<EnsureReviewItemExistsUseCase>();
builder.Services.AddScoped<GetDueReviewItemsUseCase>();
builder.Services.AddScoped<CompleteReviewItemUseCase>();
builder.Services.AddScoped<EnglishCoach.Application.Ports.ISpeechTranscriptionService, EnglishCoach.Infrastructure.AI.FakeAdapters.FakeTranscriptionService>();
builder.Services.AddScoped<EnglishCoach.Application.Ports.ISpeakingFeedbackService, EnglishCoach.Infrastructure.AI.FakeAdapters.FakeFeedbackService>();
builder.Services.AddScoped<EnglishCoach.Application.Ports.IRoleplayResponseService, EnglishCoach.Infrastructure.AI.FakeAdapters.FakeRoleplayService>();
builder.Services.AddSingleton<IClock, SystemClock>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");

app.MapGet("/health", () =>
{
    return Results.Ok(new HealthResponse("ok"));
})
.WithName("GetHealth")
.WithOpenApi();

app.MapGet("/me/profile", async (
    HttpContext httpContext,
    GetMyProfileUseCase useCase,
    CancellationToken cancellationToken) =>
{
    var userId = RequireUserId(httpContext);

    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var response = await useCase.ExecuteAsync(userId, cancellationToken);

    return response is null ? Results.NotFound() : Results.Ok(response);
})
.WithName("GetMyProfile")
.WithOpenApi();

app.MapPut("/me/profile", async (
    HttpContext httpContext,
    UpdateMyProfileRequest request,
    UpdateMyProfileUseCase useCase,
    CancellationToken cancellationToken) =>
{
    var userId = RequireUserId(httpContext);

    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var validationErrors = ValidateRequest(request);

    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    var response = await useCase.ExecuteAsync(userId, request, cancellationToken);
    return Results.Ok(response);
})
.WithName("UpdateMyProfile")
.WithOpenApi();

app.MapPost("/me/roleplay/start", async (
    HttpContext httpContext,
    EnglishCoach.Contracts.Roleplay.StartRoleplayRequest request,
    StartRoleplaySessionUseCase useCase,
    CancellationToken cancellationToken) =>
{
    var userId = RequireUserId(httpContext);
    if (userId is null) return Results.Unauthorized();

    var response = await useCase.ExecuteAsync(userId, request, cancellationToken);
    return Results.Ok(response);
})
.WithName("StartRoleplay")
.WithOpenApi();

app.MapPost("/me/roleplay/{sessionId}/turn", async (
    HttpContext httpContext,
    Guid sessionId,
    EnglishCoach.Contracts.Roleplay.RecordTurnRequest request,
    RecordTurnUseCase useCase,
    CancellationToken cancellationToken) =>
{
    var userId = RequireUserId(httpContext);
    if (userId is null) return Results.Unauthorized();

    var response = await useCase.ExecuteAsync(userId, sessionId, request, cancellationToken);
    return Results.Ok(response);
})
.WithName("RecordRoleplayTurn")
.WithOpenApi();

app.MapPost("/me/roleplay/{sessionId}/finalize", async (
    HttpContext httpContext,
    Guid sessionId,
    FinalizeRoleplayUseCase useCase,
    CancellationToken cancellationToken) =>
{
    var userId = RequireUserId(httpContext);
    if (userId is null) return Results.Unauthorized();

    var response = await useCase.ExecuteAsync(userId, sessionId, cancellationToken);
    return Results.Ok(response);
})
.WithName("FinalizeRoleplay")
.WithOpenApi();

app.MapPost("/me/speaking/attempt", async (
    HttpContext httpContext,
    EnglishCoach.Contracts.Speaking.CreateSpeakingAttemptRequest request,
    CreateSpeakingAttemptUseCase useCase,
    CancellationToken cancellationToken) =>
{
    var userId = RequireUserId(httpContext);
    if (userId is null) return Results.Unauthorized();

    var attemptId = await useCase.ExecuteAsync(userId, request.ContentItemId, request.InitialTranscript, cancellationToken);
    return Results.Ok(new EnglishCoach.Contracts.Speaking.CreateSpeakingAttemptResponse(attemptId));
})
.WithName("CreateSpeakingAttempt")
.WithOpenApi();

app.MapPost("/me/speaking/attempt/{attemptId}/evaluate", async (
    HttpContext httpContext,
    Guid attemptId,
    SubmitSpeakingAttemptEvaluationUseCase useCase,
    CancellationToken cancellationToken) =>
{
    var userId = RequireUserId(httpContext);
    if (userId is null) return Results.Unauthorized();

    var feedback = await useCase.ExecuteAsync(userId, attemptId, cancellationToken);
    
    // Map domain feedback to contract
    var response = new EnglishCoach.Contracts.Speaking.SubmitSpeakingEvaluationResponse(
        feedback.TopMistakes,
        feedback.ImprovedAnswer,
        feedback.PhrasesToReview,
        feedback.RetryPrompt
    );

    return Results.Ok(response);
})
.WithName("EvaluateSpeakingAttempt")
.WithOpenApi();

app.MapGet("/me/reviews/due", async (
    HttpContext httpContext,
    GetDueReviewItemsUseCase useCase,
    CancellationToken cancellationToken) =>
{
    var userId = RequireUserId(httpContext);

    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var response = await useCase.ExecuteAsync(userId, cancellationToken);
    return Results.Ok(response);
})
.WithName("GetDueReviewItems")
.WithOpenApi();

app.MapPost("/me/reviews/{reviewItemId}/complete", async (
    HttpContext httpContext,
    string reviewItemId,
    CompleteReviewItemRequest request,
    CompleteReviewItemUseCase useCase,
    CancellationToken cancellationToken) =>
{
    var userId = RequireUserId(httpContext);

    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var validationErrors = ValidateRequest(request);

    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    try
    {
        var response = await useCase.ExecuteAsync(userId, reviewItemId, request, cancellationToken);
        return Results.Ok(response);
    }
    catch (InvalidOperationException exception) when (exception.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
    {
        return Results.NotFound();
    }
})
.WithName("CompleteReviewItem")
.WithOpenApi();

var defaultPhraseId = "11111111-1111-1111-1111-111111111111";
var defaultScenarioId = "22222222-2222-2222-2222-222222222222";

app.MapGet("/learning-content/phrases", () => Results.Ok(new[]
{
    new
    {
        id = defaultPhraseId,
        content = "Let's touch base on this.",
        meaning = "Discuss this topic together.",
        category = "Meetings",
        difficulty = "Intermediate",
        function = "Meetings"
    }
}));

app.MapGet("/learning-content/scenarios", () => Results.Ok(new[]
{
    new
    {
        id = defaultScenarioId,
        title = "Client Interview",
        goal = "Introduce a project and clarify expectations.",
        category = "Client Communication",
        difficulty = "Advanced",
        persona = "Client stakeholder"
    }
}));

app.MapGet("/srs-reviews/due", () => Results.Ok(new[]
{
    new
    {
        id = "r1",
        phraseId = defaultPhraseId,
        content = "Clarify expectations",
        meaning = "Make expectations explicit.",
        masteryLevel = 1
    }
}));

app.MapPost("/srs-reviews/complete", (CompleteReviewRequest request) => Results.Ok());

app.MapGet("/progress/daily-mission", () => Results.Ok(new
{
    date = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"),
    missions = new[]
    {
        new
        {
            id = "m1",
            type = "review",
            title = "Complete 5 reviews",
            description = "Review due phrases for today.",
            isCompleted = false,
            count = 5
        }
    }
}));

app.MapGet("/progress/readiness", () => Results.Ok(new
{
    overallScore = 75,
    date = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"),
    version = "v1",
    trend = "improving",
    capabilities = new[]
    {
        new
        {
            area = "Grammar",
            score = 80,
            explanation = "Consistent sentence structure in meeting phrases."
        }
    }
}));

app.MapGet("/me/error-notebook/entries", async (
    HttpContext httpContext,
    GetNotebookEntriesUseCase useCase,
    CancellationToken cancellationToken) =>
{
    var userId = RequireUserId(httpContext);

    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var response = await useCase.ExecuteAsync(userId, cancellationToken);
    return Results.Ok(response);
})
.WithName("GetNotebookEntries")
.WithOpenApi();

app.MapPost("/me/error-notebook/promote", async (
    HttpContext httpContext,
    PromoteErrorPatternRequest request,
    PromoteErrorPatternUseCase useCase,
    CancellationToken cancellationToken) =>
{
    var userId = RequireUserId(httpContext);

    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var entryId = await useCase.ExecuteAsync(userId, request, cancellationToken);
    return Results.Ok(new { EntryId = entryId });
})
.WithName("PromoteErrorPattern")
.WithOpenApi();

var adminGroup = app.MapGroup("/admin")
    .AddEndpointFilter(async (invocationContext, next) =>
    {
        var httpContext = invocationContext.HttpContext;
        if (!httpContext.Request.Headers.TryGetValue("X-User-Role", out var role) || 
            !role.ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Results.StatusCode(403);
        }
        return await next(invocationContext);
    });

adminGroup.MapGet("/content/phrases", () => Results.Ok(new[]
{
    new
    {
        id = "ap1",
        content = "Draft phrase",
        meaning = "A draft phrase for admin review.",
        category = "Meetings",
        difficulty = "Beginner",
        function = "Meetings",
        status = "draft"
    }
}));

adminGroup.MapGet("/content/scenarios", () => Results.Ok(new[]
{
    new
    {
        id = "as1",
        title = "Draft scenario",
        goal = "Practice a client update.",
        category = "Client Communication",
        difficulty = "Intermediate",
        persona = "Project manager",
        status = "draft"
    }
}));

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EnglishCoach.Infrastructure.Persistence.EnglishCoachDbContext>();
    
    // Seed phrase if missing
    var phraseId = "11111111-1111-1111-1111-111111111111";
    if (!dbContext.Phrases.Any(p => p.Id == phraseId))
    {
        var phrase = EnglishCoach.Domain.Curriculum.Phrase.Create(
            phraseId,
            "Let's touch base on this.",
            "Discuss this topic together.",
            EnglishCoach.Domain.Curriculum.CommunicationFunction.Standup,
            EnglishCoach.Domain.Curriculum.ContentLevel.Core,
            "Let's touch base on this project next week."
        );
        phrase.SubmitForReview();
        phrase.Publish();
        dbContext.Phrases.Add(phrase);
    }

    // Seed scenario if missing
    var scenarioId = "22222222-2222-2222-2222-222222222222";
    if (!dbContext.RoleplayScenarios.Any(s => s.Id == scenarioId))
    {
        var scenario = EnglishCoach.Domain.Curriculum.RoleplayScenario.Create(
            scenarioId,
            "Client Interview",
            "Introduce a project and clarify expectations.",
            "Developer",
            "Client stakeholder",
            "Introduce a project and clarify expectations.",
            new[] { "Greeting", "Project Timeline" },
            new[] { phraseId },
            new[] { "Polite greeting", "Clear explanation" },
            3
        );
        scenario.SubmitForReview();
        scenario.Publish();
        dbContext.RoleplayScenarios.Add(scenario);
    }
    
    dbContext.SaveChanges();
}

app.Run();

static string? RequireUserId(HttpContext httpContext)
{
    if (httpContext.Request.Headers.TryGetValue("X-User-Id", out var values))
    {
        return values.ToString();
    }
    
    // Fallback for local UI testing
    return "test-user-123";
}

static Dictionary<string, string[]> ValidateRequest<T>(T request)
{
    var validationContext = new ValidationContext(request!);
    var validationResults = new List<ValidationResult>();

    Validator.TryValidateObject(request!, validationContext, validationResults, validateAllProperties: true);

    return validationResults
        .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty), (result, memberName) => new
        {
            MemberName = memberName,
            result.ErrorMessage
        })
        .GroupBy(item => item.MemberName)
        .ToDictionary(
            group => group.Key,
            group => group
                .Select(item => item.ErrorMessage ?? "Validation error.")
                .ToArray());
}

internal sealed record HealthResponse(string Status);
internal sealed record CompleteReviewRequest(string ReviewItemId, int Quality);
public partial class Program;
