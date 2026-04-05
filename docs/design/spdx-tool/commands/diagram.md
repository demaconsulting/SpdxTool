# DemaConsulting.SpdxTool diagram Command Design

## Purpose

The `diagram` command generates a Mermaid entity-relationship diagram from the
relationships in an SPDX document. Only package-to-package relationships are
rendered. Tool relationships (`BUILD_TOOL_OF`, `DEV_TOOL_OF`, `TEST_TOOL_OF`) are
excluded by default unless the `tools` option is specified.

## Arguments / Inputs

### Command-line usage

```text
spdx-tool diagram <spdx.json> <mermaid.txt> [tools]
```

- `spdx.json` — SPDX document to read
- `mermaid.txt` — Output file for the Mermaid diagram
- `tools` — Optional flag; includes tool relationships in the diagram

### Workflow YAML usage

```yaml

- command: diagram

  inputs:
    spdx: <spdx.json>             # SPDX file name (required)
    mermaid: <mermaid.txt>        # Output Mermaid file (required)
    tools: true                   # Optional: include tools (default: false)
```

## Implementation

1. Loads the SPDX document from `spdx.json`.
2. Initializes a `StringBuilder` with `erDiagram` as the opening line.
3. Filters relationships:
   - Excludes tool relationships unless `tools = true`.
   - Retains only relationships where both `Id` and `RelatedSpdxElement` resolve

     to `SpdxPackage` elements in the document.

4. For each retained relationship, determines the direction (Parent, Child, Sibling)

   and writes a Mermaid edge of the form:

   ```text
   "from.Name / from.Version" ||--|| "to.Name / to.Version" : "TYPE"
   ```

5. Writes the resulting diagram string to the output file via `File.WriteAllText`.

## Error Handling

| Condition | Exception |
| :--- | :--- |
| Fewer than 2 CLI arguments | `CommandUsageException` |
| Unknown CLI option | `CommandUsageException` |
| Invalid `tools` value (workflow) | `YamlException` |
| Missing `spdx` input (workflow) | `YamlException` |
| Missing `mermaid` input (workflow) | `YamlException` |
| Relationship direction is unknown | `InvalidDataException` (internal guard) |

## Constraints

- Only package-to-package relationships appear in the diagram; file or snippet

  relationships are silently skipped.

- Package labels use the format `"Name / Version"`.
- The output file is overwritten unconditionally.
- Variable expansion is applied to all string inputs via `GetMapString`.
