# DemaConsulting.SpdxTool ValidateDiagram SelfTest Design

## Purpose

`ValidateDiagram.cs` exercises the `diagram` command end-to-end within the SelfTest
subsystem. It verifies that a Mermaid diagram can be generated from an SPDX document
and that the output file is created with expected content.

## Test: `SpdxTool_Diagram`

### Setup

1. Creates a `validate.tmp` working directory.
2. Writes an SPDX JSON document containing packages with relationships suitable

   for diagram generation.

### Execution

Calls `Validate.RunSpdxTool("validate.tmp", ["--silent", "diagram", ...])` or via
a workflow YAML.

### Verification

- Checks that the output Mermaid file exists and is non-empty.
- Verifies that the file contains `erDiagram` as expected.

### Teardown

Deletes the `validate.tmp` directory.

## Error Handling

- Returns `false` if `RunSpdxTool` returns a non-zero exit code.
- Returns `false` if the output file does not exist or lacks expected content.
- The result is recorded in the `TestResults` collection as `Passed` or `Failed`.

## Constraints

- The test is self-contained; all fixture data is embedded as string literals.
- The temporary directory is always deleted in a `finally` block.
