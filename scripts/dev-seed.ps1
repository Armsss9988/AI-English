#!/usr/bin/env pwsh
# dev-seed.ps1 — Reseed the local development database without dropping it
# This runs the API briefly to trigger DatabaseCurriculumSeeder.

$ErrorActionPreference = "Stop"

# ── Production guard ──
$env_val = $env:ASPNETCORE_ENVIRONMENT
if ($env_val -and $env_val -ne "Development") {
    Write-Host "ERROR: ASPNETCORE_ENVIRONMENT is '$env_val' (not 'Development'). Refusing to seed." -ForegroundColor Red
    exit 1
}

Write-Host "=== Dev Database Seed ===" -ForegroundColor Cyan
Write-Host "Target: englishcoach_dev on localhost:9999" -ForegroundColor Yellow
Write-Host ""

# ── Ensure migrations are up to date ──
Write-Host "Step 1: Ensuring migrations are applied..." -ForegroundColor Yellow
& dotnet ef database update `
    -p "$PSScriptRoot\..\apps\api\src\EnglishCoach.Infrastructure\EnglishCoach.Infrastructure.csproj" `
    -s "$PSScriptRoot\..\apps\api\src\EnglishCoach.Api\EnglishCoach.Api.csproj"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Migration failed. Is PostgreSQL running on localhost:9999?" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 2: Seed content..." -ForegroundColor Yellow
Write-Host "  DatabaseCurriculumSeeder runs automatically when the API starts." -ForegroundColor DarkYellow
Write-Host "  Existing content is preserved (seeder uses upsert/skip logic)." -ForegroundColor DarkYellow
Write-Host ""
Write-Host "  Start the API to trigger seeding:" -ForegroundColor Cyan
Write-Host "    dotnet run --project apps/api/src/EnglishCoach.Api" -ForegroundColor White
Write-Host ""
Write-Host "✓ Migrations applied. Start the API to complete seeding." -ForegroundColor Green
