# DemaConsulting.SpdxTool get-version Command Design

## Purpose

The `get-version` command retrieves the version string of a package in an SPDX
document, matched by criteria. It is available from the command-line and from
workflow YAML files.

## Arguments / Inputs

### Command-line usage

```text
spdx-tool get-version <spdx.json> [criteria]
```

Criteria are key=value pairs (same as `find-package`):

- `id=<id>`, `name=<name>`, `version=<version>`, `filename=<filename>`,

  `download=<url>`

### Workflow YAML usage

```yaml

- command: get-version

  inputs:
    output: <variable>            # Output variable to store the version (required)
    spdx: <spdx.json>             # SPDX file name (required)
    id: <id>                      # Optional package ID criterion
    name: <name>                  # Optional package name criterion
    version: <version>            # Optional package version criterion
    filename: <filename>          # Optional package filename criterion
    download: <url>               # Optional download location criterion
```

## Implementation

1. Delegates criteria parsing to `FindPackage.ParseCriteria`.
2. Calls `FindPackage.FindPackageByCriteria` to locate the matching package.
3. Reads the `Version` property from the returned package.
4. CLI path: writes the version (or empty string if null) to the console.
5. Workflow path: stores the version in `variables[output]`.

The `output` field is read in the workflow path _after_ finding the package to
allow the `spdx` and criterion inputs to be validated first.

## Error Handling

| Condition | Exception |
| :--- | :--- |
| Fewer than 2 CLI arguments | `CommandUsageException` |
| Missing `spdx` input (workflow) | `YamlException` |
| Missing `output` input (workflow) | `YamlException` |
| Package not found or multiple matches | `CommandErrorException` (from `FindPackage`) |

## Constraints

- The version is returned as an empty string when the package has no version set.
- All `FindPackage` constraints apply (wildcard matching, etc.).
- Variable expansion is applied to all string inputs via `GetMapString`.
