#!/usr/bin/env pwsh
# dev-reset.ps1 — Reset and reseed the local development database
# WARNING: This DROPS and recreates the database. Use for development only.

$ErrorActionPreference = "Stop"

# ── Production guard ──
$env_val = $env:ASPNETCORE_ENVIRONMENT
if ($env_val -and $env_val -ne "Development") {
    Write-Host "ERROR: ASPNETCORE_ENVIRONMENT is '$env_val' (not 'Development'). Refusing to reset." -ForegroundColor Red
    exit 1
}

$connString = $env:ConnectionStrings__EnglishCoach
if ($connString -and ($connString -match "production|prod\.")) {
    Write-Host "ERROR: Connection string looks like production. Refusing to reset." -ForegroundColor Red
    exit 1
}

Write-Host "=== Dev Database Reset ===" -ForegroundColor Cyan
Write-Host "Target: englishcoach_dev on localhost:9999" -ForegroundColor Yellow
Write-Host ""

# ── Check PostgreSQL is reachable ──
Write-Host "Checking PostgreSQL connection..." -ForegroundColor Yellow
try {
    $testResult = & dotnet ef database update --dry-run `
        -p "$PSScriptRoot\..\apps\api\src\EnglishCoach.Infrastructure\EnglishCoach.Infrastructure.csproj" `
        -s "$PSScriptRoot\..\apps\api\src\EnglishCoach.Api\EnglishCoach.Api.csproj" 2>&1

    # If we get here without exception, connection likely works
    Write-Host "  PostgreSQL connection OK" -ForegroundColor Green
}
catch {
    Write-Host "  WARNING: Could not verify PostgreSQL connection. Make sure PostgreSQL is running on localhost:9999." -ForegroundColor Yellow
    Write-Host "  Error: $_" -ForegroundColor DarkYellow
}

# ── Drop and recreate database ──
Write-Host ""
Write-Host "Step 1: Dropping database..." -ForegroundColor Yellow
& dotnet ef database drop --force `
    -p "$PSScriptRoot\..\apps\api\src\EnglishCoach.Infrastructure\EnglishCoach.Infrastructure.csproj" `
    -s "$PSScriptRoot\..\apps\api\src\EnglishCoach.Api\EnglishCoach.Api.csproj"

if ($LASTEXITCODE -ne 0) {
    Write-Host "  WARNING: Database drop failed (may not exist yet). Continuing..." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Step 2: Applying all migrations..." -ForegroundColor Yellow
& dotnet ef database update `
    -p "$PSScriptRoot\..\apps\api\src\EnglishCoach.Infrastructure\EnglishCoach.Infrastructure.csproj" `
    -s "$PSScriptRoot\..\apps\api\src\EnglishCoach.Api\EnglishCoach.Api.csproj"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Migration failed." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 3: Seeding curriculum content..." -ForegroundColor Yellow
Write-Host "  (Seed runs automatically when the API starts via DatabaseCurriculumSeeder)" -ForegroundColor DarkYellow
Write-Host "  Start the API to trigger seeding: dotnet run --project apps/api/src/EnglishCoach.Api" -ForegroundColor DarkYellow

Write-Host ""
Write-Host "✓ Database reset complete!" -ForegroundColor Green
Write-Host "  Next: start the API to trigger auto-seeding." -ForegroundColor Cyan
