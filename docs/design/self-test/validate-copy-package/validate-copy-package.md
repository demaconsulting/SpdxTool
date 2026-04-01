# DemaConsulting.SpdxTool ValidateCopyPackage SelfTest Design

## Purpose

`ValidateCopyPackage.cs` exercises the `copy-package` command end-to-end within
the SelfTest subsystem. It verifies that a package can be copied from one SPDX
document to another, with the destination document updated correctly.

## Test: `SpdxTool_CopyPackage`

### Setup

1. Creates a `validate.tmp` working directory.
2. Writes two SPDX JSON documents: a source document containing the package to

   copy, and a destination document to receive the copy.

3. Writes a workflow YAML that executes the `copy-package` command.

### Execution

Calls `Validate.RunSpdxTool("validate.tmp", ["--silent", "run-workflow", "workflow.yaml"])`.

### Verification

Reads the destination SPDX document and verifies that the copied package is present
with the expected ID and metadata.

### Teardown

Deletes the `validate.tmp` directory.

## Error Handling

- Returns `false` if `RunSpdxTool` returns a non-zero exit code.
- Returns `false` if the destination document does not contain the expected package.
- The result is recorded in the `TestResults` collection as `Passed` or `Failed`.

## Constraints

- The test is self-contained; all fixture data is embedded as string literals.
- The temporary directory is always deleted in a `finally` block.
