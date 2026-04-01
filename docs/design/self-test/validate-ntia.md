# DemaConsulting.SpdxTool ValidateNtia SelfTest Design

## Purpose

`ValidateNtia.cs` exercises the NTIA minimum elements validation within the
SelfTest subsystem. It verifies that the `validate` command correctly identifies
NTIA compliance or non-compliance in SPDX documents.

## Test: `SpdxTool_Ntia`

### Setup

1. Creates a `validate.tmp` working directory.
2. Writes SPDX JSON documents — one NTIA-compliant and one non-compliant.

### Execution

1. Calls `Validate.RunSpdxTool("validate.tmp", ["--silent", "validate", "<compliant.spdx.json>", "ntia"])`.
2. Calls `Validate.RunSpdxTool("validate.tmp", ["--silent", "validate", "<noncompliant.spdx.json>", "ntia"])`.

### Verification

- Compliant document: exit code must be 0.
- Non-compliant document: exit code must be non-zero.

### Teardown

Deletes the `validate.tmp` directory.

## Error Handling

- Returns `false` if either sub-test produces an unexpected exit code.
- The result is recorded in the `TestResults` collection as `Passed` or `Failed`.

## Constraints

- The test is self-contained; all fixture data is embedded as string literals.
- The temporary directory is always deleted in a `finally` block.
