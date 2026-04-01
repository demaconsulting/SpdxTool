# DemaConsulting.SpdxTool ValidateAddRelationship SelfTest Design

## Purpose

`ValidateAddRelationship.cs` exercises the `add-relationship` command end-to-end
within the SelfTest subsystem. It verifies that a relationship can be added between
existing SPDX elements, both via command-line and via workflow, and that the resulting
document contains the expected relationship entries.

## Test: `SpdxTool_AddRelationship`

### Setup

1. Creates a `validate.tmp` working directory.
2. Writes a minimal SPDX JSON document containing two packages.
3. Writes a workflow YAML that executes `add-relationship` to add a relationship

   between the two packages.

### Execution

Calls `Validate.RunSpdxTool("validate.tmp", [...])` with the appropriate arguments.

### Verification

Reads the modified SPDX document and verifies:

- The expected relationship exists with the correct type and element IDs.

### Teardown

Deletes the `validate.tmp` directory.

## Error Handling

- Returns `false` if `RunSpdxTool` returns a non-zero exit code.
- Returns `false` if the document structure does not match expectations.
- The result is recorded in the `TestResults` collection as `Passed` or `Failed`.

## Constraints

- The test is self-contained; all fixture data is embedded as string literals.
- The temporary directory is always deleted in a `finally` block.
