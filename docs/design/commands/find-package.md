# DemaConsulting.SpdxTool find-package Command Design

## Purpose

The `find-package` command searches an SPDX document for a package matching
specified criteria and returns the package's SPDX element ID. Criteria support
wildcard patterns for flexible matching. The command is available from the
command-line and from workflow YAML files.

## Arguments / Inputs

### Command-line usage

```text
spdx-tool find-package <spdx.json> [criteria]
```

Criteria are key=value pairs:

- `id=<id>` — Match by SPDX element ID
- `name=<name>` — Match by package name
- `version=<version>` — Match by package version
- `filename=<filename>` — Match by package file name
- `download=<url>` — Match by download location

### Workflow YAML usage

```yaml

- command: find-package

  inputs:
    output: <variable>            # Output variable to store the package ID (required)
    spdx: <spdx.json>             # SPDX file name (required)
    id: <id>                      # Optional package ID criterion
    name: <name>                  # Optional package name criterion
    version: <version>            # Optional package version criterion
    filename: <filename>          # Optional package filename criterion
    download: <url>               # Optional download location criterion
```

## Implementation

1. Loads the SPDX document from `spdx.json`.
2. `ParseCriteria` populates a `Dictionary<string, string>` from the inputs.
   - CLI path: splits each `key=value` argument.
   - Workflow path: reads named fields from the YAML map.
3. `FindPackageByCriteria` iterates over `doc.Packages` and calls `IsPackageMatch`

   for each package.

4. `IsPackageMatch` evaluates each criterion against the corresponding package

   field using `Wildcard.IsMatch`.

5. Exactly one match must exist; zero or multiple matches raise `CommandErrorException`.
6. CLI path: writes the package ID to the console via `context.WriteLine`.
7. Workflow path: stores the package ID in `variables[output]`.

## Error Handling

| Condition | Exception |
| :--- | :--- |
| Fewer than 2 CLI arguments | `CommandUsageException` |
| Invalid `key=value` format in CLI criterion | `CommandUsageException` |
| Missing `output` input (workflow) | `YamlException` |
| Missing `spdx` input (workflow) | `YamlException` |
| No package matching criteria | `CommandErrorException` |
| Multiple packages matching criteria | `CommandErrorException` |

## Constraints

- All criteria are optional; with no criteria all packages match (multiple match

  error if more than one package exists).

- Criterion values support wildcard patterns (`*`, `?`) via `Wildcard.IsMatch`.
- The `version` and `filename` criteria only match packages that have those fields set.
- Variable expansion is applied to all string inputs via `GetMapString`.
