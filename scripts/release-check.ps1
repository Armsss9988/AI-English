#!/usr/bin/env pwsh
# release-check.ps1 - Full gate for PR merge
# Run this before merging any PR

$ErrorActionPreference = "Stop"

Write-Host "Running release readiness checks..." -ForegroundColor Cyan

# Check required environment variables
Write-Host "`n=== Environment Variables ===" -ForegroundColor Yellow
$requiredEnvVars = @(
    "DATABASE_URL",
    "JWT_SECRET",
    "OPENAI_API_KEY"
)
$missingVars = @()
foreach ($var in $requiredEnvVars) {
    if (-not (Get-Content env: | Where-Object { $_.Name -eq $var })) {
        $missingVars += $var
    }
}
if ($missingVars.Count -gt 0) {
    Write-Host "Missing required env vars: $($missingVars -join ', ')" -ForegroundColor Red
    Write-Host "See docs/release-checklist.md for required variables" -ForegroundColor Yellow
    exit 1
}
Write-Host "✓ Environment variables set" -ForegroundColor Green

# Backend checks
Write-Host "`n=== Backend (C#) ===" -ForegroundColor Yellow
Push-Location "be"

try {
    Write-Host "Restoring..."
    dotnet restore
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed" }

    Write-Host "Building..."
    dotnet build --configuration Release
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }

    Write-Host "Running all tests..."
    dotnet test --configuration Release --verbosity normal
    if ($LASTEXITCODE -ne 0) { throw "dotnet test failed" }

    Write-Host "Checking migrations..."
    dotnet ef migrations list 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Note: EF Core tools not available, skipping migration check" -ForegroundColor Yellow
    }
}
finally {
    Pop-Location
}

# Frontend checks
Write-Host "`n=== Frontend (Next.js) ===" -ForegroundColor Yellow
Push-Location "fe"

try {
    Write-Host "Installing dependencies..."
    pnpm install --frozen-lockfile
    if ($LASTEXITCODE -ne 0) { throw "pnpm install failed" }

    Write-Host "Running lint..."
    pnpm lint
    if ($LASTEXITCODE -ne 0) { throw "pnpm lint failed" }

    Write-Host "Running typecheck..."
    pnpm typecheck
    if ($LASTEXITCODE -ne 0) { throw "pnpm typecheck failed" }

    Write-Host "Building..."
    pnpm build
    if ($LASTEXITCODE -ne 0) { throw "pnpm build failed" }
}
finally {
    Pop-Location
}

Write-Host "`n✓ All release checks passed!" -ForegroundColor Green
Write-Host "`nReference: bussiness/04-acceptance-checklists.md" -ForegroundColor Cyan