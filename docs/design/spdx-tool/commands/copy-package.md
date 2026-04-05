# DemaConsulting.SpdxTool copy-package Command Design

## Purpose

The `copy-package` command copies a package (and optionally its children and files)
from one SPDX document to another. It is available both from the command-line and
from a workflow YAML file. If the target package already exists in the destination,
it is enhanced (merged) rather than duplicated.

## Arguments / Inputs

### Command-line usage

```text
spdx-tool copy-package <from.spdx.json> <to.spdx.json> <package> [recursive] [files]
```

- `from.spdx.json` — Source SPDX document
- `to.spdx.json` — Destination SPDX document
- `package` — Package ID to copy
- `recursive` — Optional flag; copies child packages recursively
- `files` — Optional flag; copies associated files

### Workflow YAML usage

```yaml

- command: copy-package

  inputs:
    from: <from.spdx.json>        # Source SPDX file (required)
    to: <to.spdx.json>            # Destination SPDX file (required)
    package: <package>            # Package ID to copy (required)
    recursive: true               # Optional recursive copy (default: false)
    files: true                   # Optional copy files (default: false)
    relationships:                # Optional relationships to add in destination

    - type: <relationship>        # Relationship type

      element: <element>          # Related element ID
      comment: <comment>          # Optional comment
```

## Implementation

1. Validates that `package` is not empty and not `"SPDXRef-DOCUMENT"`.
2. Loads both source (`fromDoc`) and destination (`toDoc`) SPDX documents.
3. Calls `Copy(fromDoc, toDoc, packageId, files)` to copy or enhance the package:
   - Looks up the source package by ID; raises `CommandErrorException` if not found.
   - If a same-identity package exists in the destination, it is enhanced and

     renamed; otherwise a deep copy is appended with `FilesAnalyzed = false`.

   - When `files = true` and the source package has analyzed files, each file is

     also copied or enhanced in the destination.

4. Calls `AddRelationship.Add(toDoc, relationships)` to add any new relationships.
5. If `recursive = true`, calls `CopyChildren` to recursively copy child packages

   determined by `GetChild` (based on relationship direction).

6. Saves the destination document.

### Helper: `GetChild`

Determines whether a relationship implies a child package given a parent ID, based
on `RelationshipDirection` (Parent, Child, or Sibling).

## Error Handling

| Condition | Exception |
| :--- | :--- |
| Fewer than 3 CLI arguments | `CommandUsageException` |
| Unknown CLI option | `CommandUsageException` |
| Empty or `SPDXRef-DOCUMENT` package ID | `CommandUsageException` |
| Missing `from` input (workflow) | `YamlException` |
| Missing `to` input (workflow) | `YamlException` |
| Missing `package` input (workflow) | `YamlException` |
| Invalid `recursive` value (workflow) | `YamlException` |
| Invalid `files` value (workflow) | `YamlException` |
| Package not found in source | `CommandErrorException` |
| File referenced by package not found in source | `CommandErrorException` |

## Constraints

- The source document is not modified; only the destination document is saved.
- Package identity comparison uses `SpdxPackage.Same` (by name and download location).
- File identity comparison uses `SpdxFile.Same`.
- Recursive copy tracks already-copied packages in a `HashSet<string>` to avoid

  infinite loops in cyclic relationship graphs.

- Variable expansion is applied to all string inputs via `GetMapString`.
