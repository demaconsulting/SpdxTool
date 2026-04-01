# DemaConsulting.SpdxTool ValidateUpdatePackage SelfTest Design

## Purpose

`ValidateUpdatePackage.cs` exercises the `update-package` command end-to-end within
the SelfTest subsystem. It verifies that package metadata fields can be updated in
an SPDX document via a workflow file.

## Test: `SpdxTool_UpdatePackage`

### Setup

1. Creates a `validate.tmp` working directory.
2. Writes an SPDX JSON document containing a package with initial metadata.
3. Writes a workflow YAML that executes the `update-package` command to change

   one or more fields (e.g., version, supplier).

### Execution

Calls `Validate.RunSpdxTool("validate.tmp", ["--silent", "run-workflow", "workflow.yaml"])`.

### Verification

Reads the modified SPDX document and verifies that the updated fields match the
new values specified in the workflow.

### Teardown

Deletes the `validate.tmp` directory.

## Error Handling

- Returns `false` if `RunSpdxTool` returns a non-zero exit code.
- Returns `false` if the updated package fields do not match expectations.
- The result is recorded in the `TestResults` collection as `Passed` or `Failed`.

## Constraints

- The test is self-contained; all fixture data is embedded as string literals.
- The temporary directory is always deleted in a `finally` block.
