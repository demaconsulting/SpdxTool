# build.ps1
# Builds and tests the SpdxTool solution.

$buildError = $false

Write-Host "Restoring dependencies..."
dotnet restore
if ($LASTEXITCODE -ne 0) { $buildError = $true }

Write-Host "Building..."
dotnet build --no-restore --configuration Release
if ($LASTEXITCODE -ne 0) { $buildError = $true }

Write-Host "Running tests..."
dotnet test --no-build --configuration Release --logger trx --results-directory artifacts/tests
if ($LASTEXITCODE -ne 0) { $buildError = $true }

Write-Host "Running self-validation..."
dotnet run --project src/DemaConsulting.SpdxTool --configuration Release --framework net10.0 --no-build -- --validate
if ($LASTEXITCODE -ne 0) { $buildError = $true }

exit ($buildError ? 1 : 0)
