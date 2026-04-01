# DemaConsulting.SpdxTool ValidateHash SelfTest Design

## Purpose

`ValidateHash.cs` exercises the `hash` command end-to-end within the SelfTest
subsystem. It verifies that a SHA-256 hash can be generated for a file and then
successfully verified.

## Test: `SpdxTool_Hash`

### Setup

1. Creates a `validate.tmp` working directory.
2. Writes a test file to hash.

### Execution

1. Calls `Validate.RunSpdxTool("validate.tmp", ["--silent", "hash", "generate", "sha256", "<file>"])`.
2. Calls `Validate.RunSpdxTool("validate.tmp", ["--silent", "hash", "verify", "sha256", "<file>"])`.

### Verification

- Both invocations must return exit code 0.
- The `.sha256` hash file must exist after generate.

### Teardown

Deletes the `validate.tmp` directory.

## Error Handling

- Returns `false` if either `RunSpdxTool` call returns a non-zero exit code.
- Returns `false` if the hash file is not created.
- The result is recorded in the `TestResults` collection as `Passed` or `Failed`.

## Constraints

- The test is self-contained; all fixture data is embedded as string literals.
- The temporary directory is always deleted in a `finally` block.
