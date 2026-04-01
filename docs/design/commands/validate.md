# DemaConsulting.SpdxTool validate Command Design

## Purpose

The `validate` command validates an SPDX document for specification conformance
issues. It optionally checks for NTIA (National Telecommunications and Information
Administration) minimum elements compliance. It is available from the command-line
and from workflow YAML files.

## Arguments / Inputs

### Command-line usage

```text
spdx-tool validate <spdx.json> [ntia]
```

- `spdx.json` — SPDX document to validate.
- `ntia` — Optional flag; enables NTIA minimum elements checking.

### Workflow YAML usage

```yaml
- command: validate
  inputs:
    spdx: <spdx.json>             # SPDX file name (required)
    ntia: true                    # Optional NTIA check (default: false)
```

## Implementation

1. Reads `spdx` and optionally `ntia` from inputs.
2. Loads the SPDX document.
3. Calls `doc.Validate(issues, ntia)` from the SPDX model to collect issues.
4. If no issues are found, returns silently.
5. If issues are found:
   - Each issue is written as a warning via `context.WriteWarning`.
   - A blank line is written.
   - A `CommandErrorException` is raised with the count of issues.

## Error Handling

| Condition | Exception |
| :--- | :--- |
| No arguments (CLI) | `CommandUsageException` |
| Missing `spdx` input (workflow) | `YamlException` |
| SPDX document has validation issues | `CommandErrorException` |

## Constraints

- The `ntia` flag is detected in CLI mode by searching for the string `"ntia"` in
  the arguments after the first (case-sensitive).
- In workflow mode, `ntia` is case-insensitively compared to `"true"`.
- All validation issues are reported as warnings before the error is raised, so the
  caller can inspect the full list of problems.
- Variable expansion is applied to all string inputs via `GetMapString`.
