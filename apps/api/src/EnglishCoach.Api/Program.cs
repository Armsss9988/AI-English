using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using EnglishCoach.Application.Identity;
using EnglishCoach.Application.Review;
using EnglishCoach.Contracts.Identity;
using EnglishCoach.Contracts.Review;
using EnglishCoach.Infrastructure.Identity;
using EnglishCoach.Infrastructure.Persistence;
using EnglishCoach.Infrastructure.Review;
using EnglishCoach.SharedKernel.Time;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<EnglishCoachDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("EnglishCoach")
        ?? "Host=localhost;Port=5432;Database=englishcoach_dev;Username=postgres;Password=postgres";

    options.UseNpgsql(connectionString);
});
builder.Services.AddScoped<ILearnerProfileRepository, LearnerProfileRepository>();
builder.Services.AddScoped<GetMyProfileUseCase>();
builder.Services.AddScoped<UpdateMyProfileUseCase>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<EnsureReviewItemExistsUseCase>();
builder.Services.AddScoped<GetDueReviewItemsUseCase>();
builder.Services.AddScoped<CompleteReviewItemUseCase>();
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

app.MapGet("/learning-content/phrases", () => Results.Ok(new[]
{
    new
    {
        id = "p1",
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
        id = "s1",
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
        phraseId = "p1",
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

app.MapGet("/error-notebook/entries", () => Results.Ok(new[]
{
    new
    {
        id = "e1",
        pattern = "wait for",
        original = "I wait your response.",
        corrected = "I wait for your response.",
        explanation = "Use 'for' after 'wait' when naming the thing expected.",
        category = "Grammar",
        recurrenceCount = 1,
        isArchived = false,
        lastSeenAt = DateTimeOffset.UtcNow.ToString("O")
    }
}));

app.MapGet("/admin/content/phrases", () => Results.Ok(new[]
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

app.MapGet("/admin/content/scenarios", () => Results.Ok(new[]
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

app.Run();

static string? RequireUserId(HttpContext httpContext)
{
    return httpContext.Request.Headers.TryGetValue("X-User-Id", out var values)
        ? values.ToString()
        : null;
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
public partial class Program;
