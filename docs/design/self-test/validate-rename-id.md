# DemaConsulting.SpdxTool ValidateRenameId SelfTest Design

## Purpose

`ValidateRenameId.cs` exercises the `rename-id` command end-to-end within the
SelfTest subsystem. It verifies that an SPDX element ID can be renamed throughout
an SPDX document, with all references updated correctly.

## Test: `SpdxTool_RenameId`

### Setup

1. Creates a `validate.tmp` working directory.
2. Writes an SPDX JSON document containing a package whose ID will be renamed.

### Execution

Calls `Validate.RunSpdxTool("validate.tmp", ["--silent", "rename-id", "<spdx.json>", "<old-id>", "<new-id>"])`.

### Verification

Reads the modified SPDX document and verifies:

- The package now has the new ID.
- All relationship references have been updated to use the new ID.

### Teardown

Deletes the `validate.tmp` directory.

## Error Handling

- Returns `false` if `RunSpdxTool` returns a non-zero exit code.
- Returns `false` if the document does not reflect the rename.
- The result is recorded in the `TestResults` collection as `Passed` or `Failed`.

## Constraints

- The test is self-contained; all fixture data is embedded as string literals.
- The temporary directory is always deleted in a `finally` block.
