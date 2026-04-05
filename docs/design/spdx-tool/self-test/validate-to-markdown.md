# DemaConsulting.SpdxTool ValidateToMarkdown SelfTest Design

## Purpose

`ValidateToMarkdown.cs` exercises the `to-markdown` command end-to-end within the
SelfTest subsystem. It verifies that an SPDX document can be converted to a
Markdown summary file.

## Test: `SpdxTool_ToMarkdown`

### Setup

1. Creates a `validate.tmp` working directory.
2. Writes an SPDX JSON document containing packages and metadata.

### Execution

Calls `Validate.RunSpdxTool("validate.tmp", ["--silent", "to-markdown", "<spdx.json>", "<out.md>"])`.

### Verification

- The workflow must complete with exit code 0.
- The output Markdown file must exist and contain expected table headers and content.

### Teardown

Deletes the `validate.tmp` directory.

## Error Handling

- Returns `false` if `RunSpdxTool` returns a non-zero exit code.
- Returns `false` if the Markdown output does not match expectations.
- The result is recorded in the `TestResults` collection as `Passed` or `Failed`.

## Constraints

- The test is self-contained; all fixture data is embedded as string literals.
- The temporary directory is always deleted in a `finally` block.
