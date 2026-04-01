# DemaConsulting.SpdxTool run-workflow Command Design

## Purpose

The `run-workflow` command executes a multi-step SPDX workflow defined in a YAML
file, a URL, or a NuGet package. It supports parameterized workflows, integrity
verification, and output extraction. It is available from the command-line and
from workflow YAML files (workflows can be nested).

## Arguments / Inputs

### Command-line usage

```text
spdx-tool run-workflow <workflow.yaml> [parameter=value] [--verbose]
```

- `workflow.yaml` or `http://...` — Workflow file path or HTTP URL.
- `parameter=value` — Optional parameters passed to the workflow.
- `--verbose` — Print workflow outputs after execution.

### Workflow YAML usage

```yaml

- command: run-workflow

  inputs:
    file: <workflow.yaml>         # Optional local workflow file
    url: <url>                    # Optional workflow URL
    nuget: <package:version>      # Optional NuGet package (requires file)
    integrity: <sha256>           # Optional SHA-256 integrity check
    parameters:                   # Optional parameter overrides
      name: <value>
    outputs:                      # Optional output extraction
      name: <variable>
```

## Implementation

### CLI path

1. Requires at least one argument (workflow path or URL).
2. Parses `--verbose` flag and `key=value` parameters.
3. Dispatches to `RunUrl` for HTTP paths or `RunFile` for local paths.
4. Optionally prints outputs if `--verbose`.

### Workflow path

1. Reads `file`, `url`, `nuget`, `integrity`, `parameters`, and `outputs`.
2. If `nuget` is specified, resolves the workflow file path from the local NuGet

   cache via `NuGetCache.EnsureCachedAsync`; `url` must not also be set; `file`
   must specify the relative path within the package.

3. Uses `PathHelpers.SafePathCombine` to prevent path traversal.
4. Dispatches to `RunFile` or `RunUrl`.
5. After execution, reads requested outputs from the returned variables map.

### `RunBytes`

1. Optionally verifies SHA-256 integrity against the provided hash.
2. Parses the YAML document.
3. Processes `parameters` section into local variables with expansion.
4. Validates that provided parameters are declared by the workflow.
5. Executes each `steps` entry by dispatching to `CommandsRegistry`.
6. Returns the resulting variables map as outputs.

## Error Handling

| Condition | Exception |
| :--- | :--- |
| No arguments (CLI) | `CommandUsageException` |
| Invalid `key=value` argument (CLI) | `CommandUsageException` |
| Both `file` and `url` specified | `YamlException` |
| Neither `file` nor `url` specified | `YamlException` |
| Both `nuget` and `url` specified | `YamlException` |
| `nuget` without `file` | `YamlException` |
| `nuget` value not `PackageName:version` format | `YamlException` |
| File not found | `CommandUsageException` |
| HTTP error fetching URL | `CommandErrorException` |
| Integrity check failure | `CommandErrorException` |
| Invalid YAML structure | `CommandErrorException` |
| Missing `steps` in workflow | `CommandErrorException` |
| Unknown command in step | `CommandUsageException` |
| Undeclared parameter | `CommandErrorException` |
| Requested output not produced | `CommandUsageException` |

## Constraints

- Workflows may be nested (a workflow step can call `run-workflow`).
- Each nested workflow has its own isolated variables scope.
- NuGet package resolution is synchronous (blocking) using `GetAwaiter().GetResult()`.
- Variable expansion is applied to step inputs via the standard `Expand` helper.
