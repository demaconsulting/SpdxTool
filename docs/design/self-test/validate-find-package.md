# DemaConsulting.SpdxTool ValidateFindPackage SelfTest Design

## Purpose

`ValidateFindPackage.cs` exercises the `find-package` command end-to-end within
the SelfTest subsystem. It verifies that a package can be located in an SPDX
document by criteria and that its ID is returned correctly.

## Test: `SpdxTool_FindPackage`

### Setup

1. Creates a `validate.tmp` working directory.
2. Writes an SPDX JSON document containing one or more packages.
3. Writes a workflow YAML that executes `find-package` with specific criteria.

### Execution

Calls `Validate.RunSpdxTool("validate.tmp", ["--silent", "run-workflow", "workflow.yaml"])`.

### Verification

Verifies that the workflow completed successfully and the found package ID matches
the expected value.

### Teardown

Deletes the `validate.tmp` directory.

## Error Handling

- Returns `false` if `RunSpdxTool` returns a non-zero exit code.
- Returns `false` if the found package ID does not match expectations.
- The result is recorded in the `TestResults` collection as `Passed` or `Failed`.

## Constraints

- The test is self-contained; all fixture data is embedded as string literals.
- The temporary directory is always deleted in a `finally` block.
