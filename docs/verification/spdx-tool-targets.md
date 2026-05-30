# DemaConsulting.SpdxTool.Targets

## Verification Approach

DemaConsulting.SpdxTool.Targets is verified with MSBuild integration tests in
`test/DemaConsulting.SpdxTool.Targets.Tests/`. The tests run `dotnet pack` against single-target
and multi-target fixture projects so the build targets are verified in the same execution model
used by consuming projects.

## Test Environment

The test environment requires a .NET SDK with MSBuild and `dotnet pack` support, access to the
fixture projects under `test/TestFixtures/`, and `spdx-tool` available on the system path so the
targets can invoke `spdx-tool run-workflow` during pack. The tests also require the ability to
inspect the generated `.nupkg` archive contents.

## Acceptance Criteria

Verification is acceptable when SBOM decoration runs for enabled single-target and multi-target
packs, when opt-out properties suppress decoration without breaking pack, and when a missing
workflow file produces a clear build error.

## Test Scenarios

**SingleTargetDecoration**: a single-target package is decorated when SBOM decoration is enabled.
This scenario is tested by `SpdxToolTargets_DecorateSbom_SingleTfm_True_DecoratesSbom`.

**MultiTargetDecoration**: a multi-target package is decorated through the outer-build targets path.
This scenario is tested by `SpdxToolTargets_DecorateSbom_MultiTfm_True_DecoratesSbom`.

**DecorationOptOut**: the targets skip decoration when `DecorateSBOM` is false. This scenario is
tested by `SpdxToolTargets_DecorateSbom_SingleTfm_False_SkipsDecoration`.

**SbomGenerationOptOut**: the targets skip decoration when `GenerateSBOM` is false. This scenario is
tested by `SpdxToolTargets_GenerateSbom_SingleTfm_False_SkipsEntirely`.

**MissingWorkflowError**: the targets stop with a clear error when the configured workflow file is
missing. This scenario is tested by `SpdxToolTargets_MissingWorkflow_SingleTfm_ReportsError`.
