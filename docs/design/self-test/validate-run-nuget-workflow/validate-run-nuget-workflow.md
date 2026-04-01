# DemaConsulting.SpdxTool ValidateRunNuGetWorkflow SelfTest Design

## Purpose

`ValidateRunNuGetWorkflow.cs` exercises the `run-workflow` command's NuGet package
support within the SelfTest subsystem. It verifies that a workflow file can be
resolved from a NuGet package in the local NuGet cache and executed successfully.

## Test: `SpdxTool_RunNuGetWorkflow`

### Setup

1. Creates a `validate.tmp` working directory.
2. Writes an SPDX JSON document as a target for the NuGet workflow.
3. Writes a workflow YAML that uses the `nuget:` input to reference a known

   NuGet package and a workflow file within it.

### Execution

Calls `Validate.RunSpdxTool("validate.tmp", ["--silent", "run-workflow", "workflow.yaml"])`.

### Verification

- The workflow must complete with exit code 0.
- The SPDX document must be updated as expected by the NuGet workflow.

### Teardown

Deletes the `validate.tmp` directory.

## Error Handling

- Returns `false` if `RunSpdxTool` returns a non-zero exit code.
- Returns `false` if the SPDX document is not updated as expected.
- The result is recorded in the `TestResults` collection as `Passed` or `Failed`.

## Constraints

- Requires internet or NuGet cache access to download/restore the NuGet package.
- The temporary directory is always deleted in a `finally` block.
- `PathHelpers.SafePathCombine` is used to prevent path traversal when resolving

  the workflow file within the NuGet package.
