# DemaConsulting.SpdxTool ValidateGetVersion SelfTest Design

## Purpose

`ValidateGetVersion.cs` exercises the `get-version` command end-to-end within
the SelfTest subsystem. It verifies that a package version can be retrieved from
an SPDX document by criteria.

## Test: `SpdxTool_GetVersion`

### Setup

1. Creates a `validate.tmp` working directory.
2. Writes an SPDX JSON document containing a package with a known version.
3. Writes a workflow YAML that executes `get-version` to retrieve the version.

### Execution

Calls `Validate.RunSpdxTool("validate.tmp", ["--silent", "run-workflow", "workflow.yaml"])`.

### Verification

Verifies that the workflow completed successfully and the extracted version matches
the known expected value.

### Teardown

Deletes the `validate.tmp` directory.

## Error Handling

- Returns `false` if `RunSpdxTool` returns a non-zero exit code.
- Returns `false` if the extracted version does not match expectations.
- The result is recorded in the `TestResults` collection as `Passed` or `Failed`.

## Constraints

- The test is self-contained; all fixture data is embedded as string literals.
- The temporary directory is always deleted in a `finally` block.
