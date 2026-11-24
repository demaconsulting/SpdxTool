# build.ps1
#
#
# PURPOSE:
#   Builds the SpdxTool solution in Release configuration, runs the unit tests, and runs the tool's
#   self-validation, either all of them (-All, the full local build and test run) or only the test
#   projects a change affects (-Changed / -Project).
#
# USAGE:
#   pwsh ./build.ps1 -Help                     Print the usage text and exit 0
#   pwsh ./build.ps1 -All                      Full local build, all tests, and self-validation
#   pwsh ./build.ps1 -Changed [-Base <ref>]    Only the test projects affected by the changes since <ref>
#                                              (default origin/main, else main) plus uncommitted changes
#   pwsh ./build.ps1 -Project Core,Targets     Only the test projects affected by the named projects
#   Add -DryRun to any scope to print the selection without building anything.
#   With no arguments the usage text is printed and the script exits 1 (a scope must be chosen).
#
#   A changed project selects every project that references it, directly or transitively, from the
#   ProjectReference graph, and files linked into another project (Compile/None/EmbeddedResource items
#   that point outside the project folder) select that project too. Changes that affect the whole
#   build (solution, project, props/targets, global.json, .editorconfig, nuget.config, tool manifest,
#   this script, scripts/ProjectScripts.psm1) fall back to the full build; repository files tests read
#   at run time select their test projects through $RuntimeReads; other changes the build does not
#   read select nothing. The self-validation (spdx-tool --validate) runs whenever the selection
#   includes the DemaConsulting.SpdxTool project (directly or through its test project).
#
# EXTENSION POINTS:
#   Search for "[PROJECT-SPECIFIC]" comments to find the designated locations
#   for adding project-specific build or test operations.
#
# CUSTOMIZATION:
#   This script belongs to the project: extend it as the project grows. Add steps at the
#   [PROJECT-SPECIFIC] points and add optional parameters that focus a run. The reusable scoping
#   logic lives in scripts/ProjectScripts.psm1, which is delivered by the Agents release zip, is
#   shared across repositories, and must never be edited here. Slow or environment-dependent test
#   tiers (GUI automation, integration) may be opt-in switches.
#   With no arguments the script prints the usage text and exits 1; -All must still build and run
#   the standard test suite (agents rely on that), and never remove or weaken a test step to make
#   it pass.

param(
    # Full local build and all tests.
    [switch]$All,

    # Build and test only the projects affected by the changed files.
    [switch]$Changed,

    # Git reference to compare against for -Changed (default: origin/main, else main).
    [string]$Base,

    # Build and test only the projects affected by these projects (full or short names, e.g. Core, Tests).
    [string[]]$Project,

    # Print the selection and exit without building.
    [switch]$DryRun,

    # Print the usage text and exit 0.
    [switch]$Help
)

Import-Module (Join-Path $PSScriptRoot 'scripts/ProjectScripts.psm1') -Force

# [PROJECT-SPECIFIC] Repository files that tests read at run time (through the repository root, not through a
# project item), mapped to the test project that reads them. Keep in step with the tests. The Targets tests
# pack the fixture projects under test/TestFixtures without a project reference to them. (Changes to the Targets
# project itself select the Targets tests further down.)
$RuntimeReads = [ordered]@{
    '^test/TestFixtures/' = 'DemaConsulting.SpdxTool.Targets.Tests'
}

# [PROJECT-SPECIFIC] Runs the tool's self-validation against the Release build (run after a successful build).
function Invoke-SelfValidation {
    Write-Host "Running self-validation..."
    dotnet run --project src/DemaConsulting.SpdxTool --configuration Release --framework net10.0 --no-build -- --validate
    return ($LASTEXITCODE -eq 0)
}

# ==============================================================================
# USAGE AND SCOPE VALIDATION
# ==============================================================================

function Show-Usage {
    Write-ScriptUsage -Script 'build.ps1' -IterateWith '-Changed (or -Project)' `
        -Purpose 'build the solution (Release) and run the unit tests.' `
        -Options ([ordered]@{
            '-All'             = 'Full local build, all tests, and self-validation; cannot be combined with another scope.'
            '-Changed'         = 'Only the test projects affected by files changed since the base, plus uncommitted files.'
            '-Base <ref>'      = 'Base reference for -Changed (default origin/main, else main).'
            '-Project <names>' = 'Only the test projects affected by these projects (full or short names, e.g. Core,Targets).'
            '-DryRun'          = 'Print what would be built, tested, and skipped, then exit 0 without running anything.'
            '-Help'            = 'Print this text and exit 0.'
        }) `
        -Examples @('-Changed', '-Project Targets', '-Changed -DryRun', '-All')
}

if ($Help) { Show-Usage; exit 0 }

$scopeProblems = Test-ScopeArguments -Base $Base -Scopes ([ordered]@{
        All     = $All.IsPresent
        Changed = $Changed.IsPresent
        Project = [bool]$Project
    })
if ($scopeProblems.Count -gt 0) {
    $scopeProblems | ForEach-Object { Write-Host "Error: $_" }
    Write-Host ""
    Show-Usage
    exit 1
}

$buildError = $false

# ==============================================================================
# SCOPED BUILD (-Changed / -Project)
# Selects the affected test projects; falls back to the full build when needed.
# ==============================================================================

$scoped = $false
if (-not $All) {
    $graph = Get-ProjectGraph

    # [PROJECT-SPECIFIC] The projects under test/TestFixtures are inputs the Targets tests pack, not test projects:
    # keep them out of the graph so they are never built or tested directly (see $RuntimeReads).
    foreach ($fixture in @($graph.Keys | Where-Object { $graph[$_].Directory -like 'test/TestFixtures/*' })) { $graph.Remove($fixture) }
    $selected = [System.Collections.Generic.HashSet[string]]::new()
    $fullReason = $null

    foreach ($single in @($Project | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })) {
        $resolved = Resolve-ProjectName $graph $single
        if (-not $resolved) {
            Write-Host "Unknown project '$single'. Known projects: $($graph.Keys -join ', ')"
            exit 1
        }
        [void]$selected.Add($resolved)
    }

    if ($Changed) {
        $changedFiles = Get-ChangedFiles -Base $Base
        if ($null -eq $changedFiles) {
            $fullReason = 'the changed files could not be determined'
        } else {
            Write-Host "Changed files: $($changedFiles.Count)"
            $fullReason = Add-ChangedProjects -Graph $graph -Files $changedFiles -Selected $selected -RuntimeReads $RuntimeReads
        }
    }

    if ($fullReason) {
        Write-Host "Full build required: $fullReason."
    } else {
        # [PROJECT-SPECIFIC] The Targets tests pack fixtures that import the Targets project's MSBuild targets without
        # a project reference, so selecting the Targets project (for example -Project Targets) selects them too.
        if ($selected.Contains('DemaConsulting.SpdxTool.Targets')) { [void]$selected.Add('DemaConsulting.SpdxTool.Targets.Tests') }

        Expand-ReferencingProjects -Graph $graph -Selected $selected

        # Test projects are built and tested; other leaf projects (tools/) are built only
        $tests = @($graph.Values | Where-Object { $selected.Contains($_.Name) -and $_.Directory -like 'test/*' })
        $tools = @($graph.Values | Where-Object { $selected.Contains($_.Name) -and $_.Directory -like 'tools/*' })
        $skipped = @($graph.Values | Where-Object { $_.Directory -like 'test/*' -and -not $selected.Contains($_.Name) })
        if ($tests.Count -eq 0 -and $tools.Count -eq 0) {
            Write-Host "Nothing to build: no changed file is read by the build."
            exit 0
        }

        Write-Host "Affected test projects (built and tested):"
        $tests | ForEach-Object { Write-Host "  $($_.Name)" }
        if ($tools.Count -gt 0) {
            Write-Host "Affected tool projects (build only):"
            $tools | ForEach-Object { Write-Host "  $($_.Name)" }
        }
        $selfValidate = $selected.Contains('DemaConsulting.SpdxTool')
        if ($selfValidate) { Write-Host "Self-validation: runs (the SpdxTool project is affected)." }
        if ($DryRun) {
            Write-Host "Skipped test projects ($($skipped.Count)): $(($skipped | ForEach-Object { $_.Name }) -join ', ')"
            Write-Host "Dry run: nothing was built or tested."
            exit 0
        }

        $scoped = $true
        foreach ($entry in $tests + $tools) {
            Write-Host "Building $($entry.Name)..."
            dotnet build $entry.Path --configuration Release
            if ($LASTEXITCODE -ne 0) { $buildError = $true }
        }

        if (-not $buildError) {
            foreach ($entry in $tests) {
                Write-Host "Running tests: $($entry.Name)..."
                dotnet test --project $entry.Path --no-build --configuration Release --report-trx --results-directory artifacts/tests
                if ($LASTEXITCODE -ne 0) { $buildError = $true }
            }
        } else {
            Write-Host "Skipping tests: the build failed."
        }

        # [PROJECT-SPECIFIC] Self-validation runs when the SpdxTool project is part of the selection
        if ($selfValidate -and -not $buildError) {
            if (-not (Invoke-SelfValidation)) { $buildError = $true }
        }
    }
}

if ($DryRun -and -not $scoped) { Write-Host "Dry run: would run the full build and all tests."; exit 0 }

# ==============================================================================
# FULL BUILD (-All, or a scoped run that needs everything)
# ==============================================================================

if (-not $scoped) {
    # Each step runs only if the steps before it succeeded, so tests never run against stale binaries
    Write-Host "Restoring dependencies..."
    dotnet restore
    if ($LASTEXITCODE -ne 0) { $buildError = $true }

    if (-not $buildError) {
        Write-Host "Building..."
        dotnet build --no-restore --configuration Release
        if ($LASTEXITCODE -ne 0) { $buildError = $true }
    }

    if (-not $buildError) {
        Write-Host "Running tests..."
        dotnet test --no-build --configuration Release --report-trx --results-directory artifacts/tests
        if ($LASTEXITCODE -ne 0) { $buildError = $true }
    } else {
        Write-Host "Skipping tests: the build failed."
    }

    # [PROJECT-SPECIFIC] Self-validation of the built tool; skipped once an earlier step failed.
    if (-not $buildError) {
        if (-not (Invoke-SelfValidation)) { $buildError = $true }
    } else {
        Write-Host "Skipping self-validation: an earlier step failed."
    }
}

if (-not $buildError) {
    Write-Host "Build and tests completed successfully!"
}

exit ($buildError ? 1 : 0)
