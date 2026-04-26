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

// Options
builder.Services.Configure<EnglishCoach.Infrastructure.AI.OpenAI.OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));

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

// Progress & Daily Mission
builder.Services.AddScoped<EnglishCoach.Domain.DailyMission.IDailyMissionDataProvider, EnglishCoach.Infrastructure.DailyMission.DailyMissionDataProvider>();
builder.Services.AddScoped<EnglishCoach.Domain.DailyMission.DailyMissionSelector>();
builder.Services.AddScoped<EnglishCoach.Application.DailyMission.GetDailyMissionQuery>();
builder.Services.AddScoped<EnglishCoach.Application.Progress.IProgressDataProvider, EnglishCoach.Infrastructure.Progress.ProgressDataProvider>();
builder.Services.AddScoped<EnglishCoach.Application.Progress.ILearnerProgressDataProvider, EnglishCoach.Infrastructure.Progress.ProgressDataProvider>();
builder.Services.AddScoped<EnglishCoach.Application.Progress.IReadinessSnapshotRepository, EnglishCoach.Infrastructure.Progress.ReadinessSnapshotRepository>();
builder.Services.AddScoped<EnglishCoach.Application.Progress.RecalculateReadinessUseCase>();
builder.Services.AddScoped<EnglishCoach.Application.Progress.GetReadinessQuery>();
builder.Services.AddScoped<EnglishCoach.Application.Progress.GetCapabilityMatrixQuery>();

// AI Providers (NIM / OpenAI)
builder.Services.AddScoped<EnglishCoach.Application.Ports.ISpeechTranscriptionService, EnglishCoach.Infrastructure.AI.OpenAI.NimTranscriptionService>();
builder.Services.AddScoped<EnglishCoach.Application.Ports.ISpeakingFeedbackService, EnglishCoach.Infrastructure.AI.OpenAI.NimSpeakingFeedbackService>();
builder.Services.AddScoped<EnglishCoach.Application.Ports.IRoleplayResponseService, EnglishCoach.Infrastructure.AI.OpenAI.NimRoleplayService>();

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

// ── Learning Content: Real DB queries ──

app.MapGet("/learning-content/phrases", async (
    IPhraseRepository phraseRepo,
    CancellationToken cancellationToken) =>
{
    var phrases = await phraseRepo.GetAllPublishedAsync(cancellationToken);
    var response = phrases.Select(p => new
    {
        id = p.Id,
        content = p.Text,
        meaning = p.ViMeaning,
        category = p.CommunicationFunction.ToString(),
        difficulty = p.Level.ToString(),
        function = p.CommunicationFunction.ToString()
    });
    return Results.Ok(response);
})
.WithName("GetPublishedPhrases")
.WithOpenApi();

app.MapGet("/learning-content/scenarios", async (
    IRoleplayScenarioRepository scenarioRepo,
    CancellationToken cancellationToken) =>
{
    var scenarios = await scenarioRepo.GetAllPublishedAsync(cancellationToken);
    var response = scenarios.Select(s => new
    {
        id = s.Id,
        title = s.Title,
        goal = s.CommunicationGoal,
        category = s.WorkplaceContext,
        difficulty = s.Difficulty switch { <= 1 => "Beginner", 2 => "Intermediate", _ => "Advanced" },
        persona = s.ClientPersona
    });
    return Results.Ok(response);
})
.WithName("GetPublishedScenarios")
.WithOpenApi();

// ── SRS Reviews: redirect to real /me/reviews/* endpoints ──
// Frontend calls /srs-reviews/due → redirected to real review use case

app.MapGet("/srs-reviews/due", async (
    HttpContext httpContext,
    GetDueReviewItemsUseCase useCase,
    CancellationToken cancellationToken) =>
{
    var userId = RequireUserId(httpContext);
    if (userId is null) return Results.Unauthorized();

    var items = await useCase.ExecuteAsync(userId, cancellationToken);
    // Map to legacy frontend shape
    var response = items.Items.Select(i => new
    {
        id = i.ReviewItemId,
        phraseId = i.ItemId,
        content = i.DisplayText,
        meaning = i.DisplaySubtitle ?? "",
        masteryLevel = i.RepetitionCount
    });
    return Results.Ok(response);
})
.WithName("GetSrsReviewsDue")
.WithOpenApi();

app.MapPost("/srs-reviews/complete", async (
    HttpContext httpContext,
    CompleteReviewLegacyRequest request,
    CompleteReviewItemUseCase useCase,
    CancellationToken cancellationToken) =>
{
    var userId = RequireUserId(httpContext);
    if (userId is null) return Results.Unauthorized();

    var qualityMap = request.Outcome?.ToLowerInvariant() switch
    {
        "easy" => "easy",
        "good" => "good",
        "hard" => "hard",
        _ => "again"
    };

    try
    {
        var completeRequest = new CompleteReviewItemRequest(qualityMap);
        await useCase.ExecuteAsync(userId, request.ReviewItemId, completeRequest, cancellationToken);
        return Results.Ok();
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
    {
        return Results.NotFound();
    }
})
.WithName("CompleteSrsReview")
.WithOpenApi();

// ── Progress & Daily Mission ──

app.MapGet("/progress/daily-mission", async (
    HttpContext httpContext,
    EnglishCoach.Application.DailyMission.GetDailyMissionQuery query,
    CancellationToken cancellationToken) =>
{
    var userId = RequireUserId(httpContext);
    if (userId is null) return Results.Unauthorized();

    if (!Guid.TryParse(userId, out var userGuid))
        return Results.BadRequest("Invalid user ID");

    var response = await query.ExecuteAsync(userGuid, cancellationToken);
    return Results.Ok(response);
})
.WithName("GetDailyMission")
.WithOpenApi();

app.MapGet("/progress/readiness", async (
    HttpContext httpContext,
    EnglishCoach.Application.Progress.GetReadinessQuery query,
    CancellationToken cancellationToken) =>
{
    var userId = RequireUserId(httpContext);
    if (userId is null) return Results.Unauthorized();

    if (!Guid.TryParse(userId, out var userGuid))
        return Results.BadRequest("Invalid user ID");

    var response = await query.ExecuteAsync(userGuid, cancellationToken);
    return Results.Ok(response);
})
.WithName("GetReadiness")
.WithOpenApi();

app.MapGet("/progress/capabilities", async (
    HttpContext httpContext,
    EnglishCoach.Application.Progress.GetCapabilityMatrixQuery query,
    CancellationToken cancellationToken) =>
{
    var userId = RequireUserId(httpContext);
    if (userId is null) return Results.Unauthorized();

    if (!Guid.TryParse(userId, out var userGuid))
        return Results.BadRequest("Invalid user ID");

    var response = await query.ExecuteAsync(userGuid, cancellationToken);
    return Results.Ok(response);
})
.WithName("GetCapabilities")
.WithOpenApi();

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

adminGroup.MapGet("/content/phrases", async (
    EnglishCoachDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var phrases = await dbContext.Phrases.ToListAsync(cancellationToken);
    var response = phrases.Select(p => new
    {
        id = p.Id,
        content = p.Text,
        meaning = p.ViMeaning,
        category = p.CommunicationFunction.ToString(),
        difficulty = p.Level.ToString(),
        function = p.CommunicationFunction.ToString(),
        status = p.State.ToString().ToLowerInvariant()
    });
    return Results.Ok(response);
})
.WithName("AdminGetPhrases")
.WithOpenApi();

adminGroup.MapGet("/content/scenarios", async (
    EnglishCoachDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var scenarios = await dbContext.RoleplayScenarios.ToListAsync(cancellationToken);
    var response = scenarios.Select(s => new
    {
        id = s.Id,
        title = s.Title,
        goal = s.CommunicationGoal,
        category = s.WorkplaceContext,
        difficulty = s.Difficulty switch { <= 1 => "Beginner", 2 => "Intermediate", _ => "Advanced" },
        persona = s.ClientPersona,
        status = s.State.ToString().ToLowerInvariant()
    });
    return Results.Ok(response);
})
.WithName("AdminGetScenarios")
.WithOpenApi();

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
        var userId = values.ToString();
        if (!string.IsNullOrWhiteSpace(userId))
            return userId;
    }
    
    // For local UI testing without real auth, return a default mock user ID
    return "00000000-0000-0000-0000-000000000001";
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
internal sealed record CompleteReviewLegacyRequest(string ReviewItemId, string? Outcome);
public partial class Program;
