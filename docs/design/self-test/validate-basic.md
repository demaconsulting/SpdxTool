# DemaConsulting.SpdxTool ValidateBasic SelfTest Design

## Purpose

`ValidateBasic.cs` exercises basic tool functionality within the SelfTest subsystem.
It verifies that the tool handles valid and invalid SPDX documents correctly using
the `validate` command.

## Test: `SpdxTool_Basic`

### Sub-tests

The test runs two sub-tests:

**DoValidateValid**: Verifies that a well-formed SPDX document passes validation
without errors (exit code 0).

**DoValidateInvalid**: Verifies that a malformed SPDX document fails validation
(non-zero exit code).

### Setup

1. Creates a `validate.tmp` working directory.
2. Writes SPDX fixture files (valid and invalid variants).

### Execution

Calls `Validate.RunSpdxTool("validate.tmp", ["--silent", "validate", ...])` for
each sub-test.

### Verification

- Valid document: exit code must be 0.
- Invalid document: exit code must be non-zero.

### Teardown

Deletes the `validate.tmp` directory.

## Error Handling

- Returns `false` if either sub-test produces an unexpected exit code.
- The result is recorded in the `TestResults` collection as `Passed` or `Failed`.

## Constraints

- The test exercises the `validate` command (not the `--validate` self-test flag).
- The temporary directory is always deleted in a `finally` block.
