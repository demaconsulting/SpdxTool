# Quality check script for local development
# This script runs all quality checks before committing code

$ErrorActionPreference = "Stop"

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
    $buildOutput = dotnet build --no-restore 2>&1
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
    $testOutput = dotnet test --no-build --verbosity quiet 2>&1
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
if ($buildOutput -match "warning") {
    Write-Host "⚠ Warnings found in build output" -ForegroundColor Yellow
    $buildOutput | Select-String "warning" | Select-Object -First 5
} else {
    Print-Status $true "No analysis warnings"
}
Write-Host ""

Write-Host "Step 6: Running self-validation..."
try {
    # Note: Using net8.0 explicitly since project targets multiple frameworks.
    # Self-validation behavior is identical across frameworks, so we use the LTS version.
    $validateOutput = dotnet run --project src/DemaConsulting.SpdxTool --no-build --framework net8.0 -- --validate 2>&1
    if ($LASTEXITCODE -eq 0 -and $validateOutput -match "Validation Passed") {
        Print-Status $true "Self-validation passed"
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
