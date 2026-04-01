# DemaConsulting.SpdxTool ValidateAddPackage SelfTest Design

## Purpose

`ValidateAddPackage.cs` exercises the `add-package` command end-to-end within the
SelfTest subsystem. It verifies that a package can be added to an SPDX document
via a workflow file, and that the resulting document contains the expected content.

## Test: `SpdxTool_AddPackage`

### Setup

1. Creates a `validate.tmp` working directory.
2. Writes a minimal SPDX JSON document (`test.spdx.json`) containing one package

   (`SPDXRef-Package-1`).

3. Writes a workflow YAML (`workflow.yaml`) that executes the `add-package` command

   to add `SPDXRef-Package-2` with a `BUILD_TOOL_OF` relationship to `SPDXRef-Package-1`,
   including a `purl` external reference.

### Execution

Calls `Validate.RunSpdxTool("validate.tmp", ["--silent", "run-workflow", "workflow.yaml"])`.

### Verification

Reads the modified SPDX document and verifies:

- Exactly two packages exist with IDs `SPDXRef-Package-1` and `SPDXRef-Package-2`.
- A `BUILD_TOOL_OF` relationship exists from `SPDXRef-Package-2` to `SPDXRef-Package-1`.

### Teardown

Deletes the `validate.tmp` directory.

## Error Handling

- Returns `false` if `RunSpdxTool` returns a non-zero exit code.
- Returns `false` if the document structure does not match expectations.
- The result is recorded in the `TestResults` collection as `Passed` or `Failed`.

## Constraints

- The test is self-contained; all fixture data is embedded as string literals.
- The temporary directory is always deleted in a `finally` block.
