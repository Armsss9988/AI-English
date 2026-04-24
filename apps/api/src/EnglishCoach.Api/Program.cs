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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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
