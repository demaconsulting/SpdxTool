# DemaConsulting.SpdxTool add-relationship Command Design

## Purpose

The `add-relationship` command adds one or more SPDX relationships between
elements in an SPDX document. Relationships can be added directly from the
command-line or from a workflow YAML file.

## Arguments / Inputs

### Command-line usage

```text
spdx-tool add-relationship <spdx.json> <id> <type> <element> [comment]
```

- `spdx.json` — SPDX document to modify
- `id` — Source element ID
- `type` — Relationship type (e.g. `DESCRIBES`, `CONTAINS`, `BUILD_TOOL_OF`)
- `element` — Related (target) element ID
- `comment` — Optional relationship comment

### Workflow YAML usage

```yaml

- command: add-relationship

  inputs:
    spdx: <spdx.json>             # SPDX file name (required)
    id: <id>                      # Source element ID (required)
    replace: false                # Replace existing relationships (default: true)
    relationships:

    - type: <relationship>        # Relationship type (required)

      element: <element>          # Related element ID (required)
      comment: <comment>          # Optional comment
```

## Implementation

### Command-line path

1. Requires at least 4 arguments; fewer raises `CommandUsageException`.
2. Builds a single `SpdxRelationship` from the positional arguments.
3. Calls `Add(spdxFile, relationships)` with `replace = false`.

### Workflow path

1. Reads `spdx`, `id`, `replace`, and `relationships` from inputs.
2. `replace` defaults to `"true"` and is parsed as a boolean.
3. Each entry in `relationships` is parsed by `Parse(command, id, node, variables)`,

   which constructs an `SpdxRelationship` using `type`, `element`, and optional
   `comment` fields.

4. Calls `Add(spdxFile, relationships, replace)`.

### Internal helpers

- `Add(string, SpdxRelationship[], bool)` — loads the SPDX document, delegates to

  `SpdxRelationships.Add`, and saves the result.

- `Add(SpdxDocument, SpdxRelationship[], bool)` — wraps `SpdxRelationships.Add`

  and converts any exception to `CommandErrorException`.

- `Parse(command, packageId, YamlSequenceNode?, variables)` — iterates nodes and

  calls the single-node overload for each.

- `Parse(command, packageId, YamlMappingNode, variables)` — reads `type`, `element`,

  and `comment` from a mapping node.

## Error Handling

| Condition | Exception |
| :--- | :--- |
| Fewer than 4 CLI arguments | `CommandUsageException` |
| Missing `spdx` input (workflow) | `YamlException` |
| Missing `id` input (workflow) | `YamlException` |
| Invalid `replace` value (workflow) | `YamlException` |
| Missing `relationships` input (workflow) | `YamlException` |
| Relationship node not a mapping | `YamlException` |
| Missing relationship `type` | `YamlException` |
| Missing relationship `element` | `YamlException` |
| Error from `SpdxRelationships.Add` | `CommandErrorException` |

## Constraints

- The `replace` flag controls whether existing relationships of the same type from

  the same source element are replaced or supplemented.

- Variable expansion is applied to all string inputs via `GetMapString`.
- Relationship types must be valid SPDX relationship type strings as defined by

  `SpdxRelationshipTypeExtensions.FromText`.
