# build.ps1
# Builds and tests the SpdxTool solution.
#
# CUSTOMIZATION:
#   This script belongs to the project: extend it as the project grows. Add steps at the
#   [PROJECT-SPECIFIC] points and add optional parameters that focus a run (for example
#   -Project <name> in a multi-library repository). Slow or environment-dependent test tiers
#   (GUI automation, integration) may be opt-in switches.
#   Run with no arguments it must still build and run the standard test suite (agents rely
#   on that), and never remove or weaken a test step to make it pass.

$buildError = $false

Write-Host "Restoring dependencies..."
dotnet restore
if ($LASTEXITCODE -ne 0) { $buildError = $true }

Write-Host "Building..."
dotnet build --no-restore --configuration Release
if ($LASTEXITCODE -ne 0) { $buildError = $true }

Write-Host "Running tests..."
dotnet test --no-build --configuration Release --report-trx --results-directory artifacts/tests
if ($LASTEXITCODE -ne 0) { $buildError = $true }

Write-Host "Running self-validation..."
dotnet run --project src/DemaConsulting.SpdxTool --configuration Release --framework net10.0 --no-build -- --validate
if ($LASTEXITCODE -ne 0) { $buildError = $true }

exit ($buildError ? 1 : 0)
