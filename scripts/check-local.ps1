#!/usr/bin/env pwsh
# check-local.ps1 - Fast pre-commit checks
# Run this before every commit

$ErrorActionPreference = "Stop"

Write-Host "Running pre-commit checks..." -ForegroundColor Cyan

# Backend checks
Write-Host "`n=== Backend (C#) ===" -ForegroundColor Yellow
Push-Location "be"

try {
    Write-Host "Restoring packages..."
    dotnet restore --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed" }

    Write-Host "Building..."
    dotnet build --no-restore --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }

    Write-Host "Running unit tests..."
    dotnet test --no-build --verbosity quiet --filter "Category=Unit"
    if ($LASTEXITCODE -ne 0) { throw "dotnet test failed" }
}
finally {
    Pop-Location
}

# Frontend checks
Write-Host "`n=== Frontend (Next.js) ===" -ForegroundColor Yellow
Push-Location "fe"

try {
    Write-Host "Installing dependencies..."
    pnpm install --frozen-lockfile 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "pnpm install failed" }

    Write-Host "Running lint..."
    pnpm lint 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "pnpm lint failed" }

    Write-Host "Running typecheck..."
    pnpm typecheck 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "pnpm typecheck failed" }
}
finally {
    Pop-Location
}

Write-Host "`n✓ All pre-commit checks passed!" -ForegroundColor Green