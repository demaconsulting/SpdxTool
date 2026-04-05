# DemaConsulting.SpdxTool rename-id Command Design

## Purpose

The `rename-id` command renames an SPDX element ID throughout an SPDX document,
updating all packages, files, snippets, relationships, and describes entries that
reference the old ID. It is available from the command-line and from workflow YAML
files.

## Arguments / Inputs

### Command-line usage

```text
spdx-tool rename-id <spdx.json> <old-id> <new-id>
```

### Workflow YAML usage

```yaml
- command: rename-id
  inputs:
    spdx: <spdx.json>             # SPDX file name (required)
    old: <old-id>                 # Old element ID (required)
    new: <new-id>                 # New element ID (required)
```

## Implementation

1. Reads `spdx`, `old`, and `new` from inputs.
2. Validates `old` and `new` IDs:
   - Neither may be empty or `"SPDXRef-DOCUMENT"`.
   - They must differ.
   - `new` must not already be in use by another element.
3. Iterates all packages, files, snippets, relationships, and document describes
   arrays, replacing every occurrence of `old` with `new` using `UpdateId`.
4. If `old == new`, returns without modification (no-op).

## Error Handling

| Condition | Exception |
| :--- | :--- |
| Not exactly 3 CLI arguments | `CommandUsageException` |
| Missing `spdx` input (workflow) | `YamlException` |
| Missing `new` input (workflow) | `YamlException` |
| Missing `old` input (workflow) | `YamlException` |
| Empty or `SPDXRef-DOCUMENT` old ID | `CommandUsageException` |
| Empty or `SPDXRef-DOCUMENT` new ID | `CommandUsageException` |
| New ID already in use | `CommandErrorException` |

## Constraints

- Renaming `old` to itself is silently treated as a no-op.
- The new ID must not collide with any existing package, file, or snippet ID.
- References in `HasFiles`, `RelatedSpdxElement`, and `Describes` arrays are all
  updated so that referential integrity is maintained.
- Variable expansion is applied to all string inputs via `GetMapString`.
