# Quality check script for local development
# This script runs all quality checks before committing code

$ErrorActionPreference = "Stop"

# Configuration
# Target framework for self-validation (LTS version recommended)
$targetFramework = if ($env:SPDX_TARGET_FRAMEWORK) { $env:SPDX_TARGET_FRAMEWORK } else { "net8.0" }

# Create unique temporary directory for this run
$tempDir = New-Item -ItemType Directory -Path (Join-Path $env:TEMP "spdx-quality-$(New-Guid)")
$cleanupScript = { Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue }
Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action $cleanupScript | Out-Null

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "SpdxTool Quality Check" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

$failed = $false

function Print-Status {
    param (
        [bool]$success,
        [string]$message
    )
    
    if ($success) {
        Write-Host "✓ $message" -ForegroundColor Green
    } else {
        Write-Host "✗ $message" -ForegroundColor Red
        $script:failed = $true
    }
}

Write-Host "Step 1: Cleaning build artifacts..."
try {
    dotnet clean | Out-Null
    Print-Status $true "Clean completed"
} catch {
    Print-Status $false "Clean failed"
}
Write-Host ""

Write-Host "Step 2: Restoring dependencies..."
try {
    dotnet restore | Out-Null
    Print-Status $true "Restore completed"
} catch {
    Print-Status $false "Restore failed"
}
Write-Host ""

Write-Host "Step 3: Building solution..."
try {
    $buildLogPath = Join-Path $tempDir "build.log"
    $buildOutput = dotnet build --no-restore 2>&1 | Tee-Object -FilePath $buildLogPath
    if ($LASTEXITCODE -eq 0) {
        Print-Status $true "Build completed"
    } else {
        Write-Host $buildOutput
        Print-Status $false "Build failed"
    }
} catch {
    Print-Status $false "Build failed"
}
Write-Host ""

Write-Host "Step 4: Running tests..."
try {
    $testLogPath = Join-Path $tempDir "test.log"
    $testOutput = dotnet test --no-build --verbosity quiet 2>&1 | Tee-Object -FilePath $testLogPath
    if ($LASTEXITCODE -eq 0) {
        Print-Status $true "All tests passed"
    } else {
        Write-Host $testOutput
        Print-Status $false "Tests failed"
    }
} catch {
    Print-Status $false "Tests failed"
}
Write-Host ""

Write-Host "Step 5: Running code analysis..."
$buildContent = Get-Content $buildLogPath -Raw
if ($buildContent -match "warning") {
    Write-Host "⚠ Warnings found in build output" -ForegroundColor Yellow
    $buildContent | Select-String "warning" | Select-Object -First 5
} else {
    Print-Status $true "No analysis warnings"
}
Write-Host ""

Write-Host "Step 6: Running self-validation..."
try {
    # Note: Using configured framework (default: net8.0 LTS) for self-validation.
    # Self-validation behavior is identical across frameworks.
    # Override with: $env:SPDX_TARGET_FRAMEWORK="net9.0"; .\scripts\quality-check.ps1
    $validateLogPath = Join-Path $tempDir "validate.log"
    $validateOutput = dotnet run --project src/DemaConsulting.SpdxTool --no-build --framework $targetFramework -- --validate 2>&1 | Tee-Object -FilePath $validateLogPath
    if ($LASTEXITCODE -eq 0 -and $validateOutput -match "Validation Passed") {
        Print-Status $true "Self-validation passed (framework: $targetFramework)"
    } else {
        Write-Host $validateOutput
        Print-Status $false "Self-validation failed"
    }
} catch {
    Print-Status $false "Self-validation failed"
}
Write-Host ""

Write-Host "==================================================" -ForegroundColor Cyan
if (-not $failed) {
    Write-Host "All quality checks passed!" -ForegroundColor Green
    Write-Host "You're ready to commit your changes."
    exit 0
} else {
    Write-Host "Some quality checks failed." -ForegroundColor Red
    Write-Host "Please fix the issues before committing."
    exit 1
}
