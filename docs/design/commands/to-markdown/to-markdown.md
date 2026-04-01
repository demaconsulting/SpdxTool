# DemaConsulting.SpdxTool to-markdown Command Design

## Purpose

The `to-markdown` command generates a Markdown summary of an SPDX document. The
summary includes document metadata, root packages, non-root packages, and tool
packages, each in a formatted table. It is available from the command-line and
from workflow YAML files.

## Arguments / Inputs

### Command-line usage

```text
spdx-tool to-markdown <spdx.json> <out.md> [title] [depth]
```

- `spdx.json` — SPDX document to summarize.
- `out.md` — Output Markdown file.
- `title` — Optional section title (default: `"SPDX Document"`).
- `depth` — Optional heading depth (integer ≥ 1, default: `2`).

### Workflow YAML usage

```yaml

- command: to-markdown

  inputs:
    spdx: <spdx.json>             # SPDX file name (required)
    markdown: <out.md>            # Output Markdown file (required)
    title: <title>                # Optional title (default: "SPDX Document")
    depth: <depth>                # Optional heading depth (default: 2)
```

## Implementation

1. Loads the SPDX document.
2. Constructs the Markdown in a `StringBuilder`:
   - A document summary table (file name, name, file/package/relationship counts,

     creation info).

   - Root packages table (packages returned by `doc.GetRootPackages()`).
   - Non-root, non-tool packages table.
   - Tools table (packages involved in `BUILD_TOOL_OF`, `DEV_TOOL_OF`,

     or `TEST_TOOL_OF` relationships).

3. Writes the result to the output file via `File.WriteAllText`.

### License helper

`License(package)` returns `ConcludedLicense` if non-empty and not `"NOASSERTION"`,
then `DeclaredLicense`, then `"NOASSERTION"` as a fallback.

## Error Handling

| Condition | Exception |
| :--- | :--- |
| Fewer than 2 CLI arguments | `CommandUsageException` |
| Empty or whitespace `title` argument (CLI) | `CommandUsageException` |
| Invalid or non-positive `depth` argument (CLI) | `CommandUsageException` |
| Missing `spdx` input (workflow) | `YamlException` |
| Missing `markdown` input (workflow) | `YamlException` |
| Empty or whitespace `title` input (workflow) | `YamlException` |
| Invalid or non-positive `depth` input (workflow) | `YamlException` |

## Constraints

- The output file is overwritten unconditionally.
- Heading depth controls the number of `#` characters in section headers

  (`depth = 2` → `##`, `depth = 3` → `###`, etc.).

- Sub-section headings use one extra `#` relative to the document heading

  (i.e. `depth + 1`).

- Variable expansion is applied to all string inputs via `GetMapString`.
