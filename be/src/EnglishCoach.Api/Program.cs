using EnglishCoach.Api.Mapping;
using EnglishCoach.Application.Ports;
using EnglishCoach.Infrastructure.AI;
using EnglishCoach.Infrastructure.Persistence;
using EnglishCoach.SharedKernel.Clock;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "EnglishCoach API", Version = "v1" });
});

// Clock
builder.Services.AddSingleton<IClock, SystemClock>();

// Database
builder.Services.AddDbContext<EnglishCoachDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories (placeholder registrations)
builder.Services.AddScoped<ISpeakingRepository, SpeakingRepository>();
builder.Services.AddScoped<IRoleplayRepository, RoleplayRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IProgressRepository, ProgressRepository>();

// AI Provider Ports
builder.Services.AddScoped<ISpeechTranscriptionService, OpenAiTranscriptionService>();
builder.Services.AddScoped<ISpeakingFeedbackService, OpenAiFeedbackService>();
builder.Services.AddScoped<IRoleplayResponseService, OpenAiRoleplayService>();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler("/health/error");
app.MapControllers();

app.Run();

public partial class Program { }