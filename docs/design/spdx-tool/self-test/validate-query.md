# DemaConsulting.SpdxTool ValidateQuery SelfTest Design

## Purpose

`ValidateQuery.cs` exercises the `query` command end-to-end within the SelfTest
subsystem. It verifies that an external program can be queried and a value extracted
from its output using a regular expression pattern.

## Test: `SpdxTool_Query`

### Setup

1. Creates a `validate.tmp` working directory.
2. Writes a workflow YAML that executes `query` against a known program

   (e.g., `dotnet --version`) to extract a version string.

### Execution

Calls `Validate.RunSpdxTool("validate.tmp", ["--silent", "run-workflow", "workflow.yaml"])`.

### Verification

- The workflow must complete with exit code 0.
- The extracted value must match the expected pattern.

### Teardown

Deletes the `validate.tmp` directory.

## Error Handling

- Returns `false` if `RunSpdxTool` returns a non-zero exit code.
- Returns `false` if the extracted value does not match expectations.
- The result is recorded in the `TestResults` collection as `Passed` or `Failed`.

## Constraints

- The test requires `dotnet` to be available on the system PATH.
- The temporary directory is always deleted in a `finally` block.
