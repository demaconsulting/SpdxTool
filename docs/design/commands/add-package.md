# DemaConsulting.SpdxTool add-package Command Design

## Purpose

The `add-package` command adds a new SPDX package to an existing SPDX document.
It supports optional relationships between the new package and other elements in
the document. If a package with the same identity already exists, the command
enhances (merges) that package rather than adding a duplicate.

## Arguments / Inputs

This command is only valid inside a workflow YAML file:

```yaml
- command: add-package
  inputs:
    spdx: <spdx.json>             # SPDX file name (required)
    package:                      # New package information (required)
      id: <id>                    # New package ID (required)
      name: <name>                # New package name (required)
      download: <download-url>    # New package download URL (required)
      version: <version>          # Optional package version
      filename: <filename>        # Optional package filename
      supplier: <supplier>        # Optional package supplier
      originator: <originator>    # Optional package originator
      homepage: <homepage>        # Optional package homepage
      copyright: <copyright>      # Optional package copyright
      summary: <summary>          # Optional package summary
      description: <description>  # Optional package description
      license: <license>          # Optional package license
      purl: <package-url>         # Optional package URL (appended as external reference)
      cpe23: <cpe-identifier>     # Optional CPE 2.3 identifier (appended as external reference)
    relationships:                # Optional relationships
    - type: <relationship>        # Relationship type (e.g. DESCRIBES, CONTAINS)
      element: <element>          # Related element ID
      comment: <comment>          # Optional comment
```

If invoked directly from the command-line (not in a workflow), the command raises
a `CommandUsageException` with an explanatory message.

## Implementation

1. The `Run(Context, YamlMappingNode, Dictionary)` override reads the `inputs` map.
2. The `spdx` input is required; its absence raises a `YamlException`.
3. The `package` sub-map is parsed by `ParsePackage`, which constructs an
   `SpdxPackage` from the YAML fields. Required fields are `id`, `name`, and
   `download`; all others default to `null` or `"NOASSERTION"` where appropriate.
4. Package ID must not be empty or `"SPDXRef-DOCUMENT"`; violation raises
   `CommandUsageException`.
5. Optional `purl` and `cpe23` inputs are appended as `SpdxExternalReference`
   entries on the package.
6. `relationships` is an optional sequence; each entry is parsed by
   `AddRelationship.Parse`.
7. `AddPackageToSpdxFile` loads the document, calls `Add` (which either enhances
   an existing same-identity package and renames it, or appends a deep copy),
   then calls `AddRelationship.Add` for each relationship, and saves the document.

## Error Handling

| Condition | Exception |
| :--- | :--- |
| Invoked from command line (not workflow) | `CommandUsageException` |
| Missing `spdx` input | `YamlException` |
| Missing `package` input | `YamlException` |
| Missing package `id` | `YamlException` |
| Empty or `SPDXRef-DOCUMENT` package ID | `CommandUsageException` |
| Missing package `name` | `YamlException` |
| Missing package `download` | `YamlException` |
| Relationship parse errors | `YamlException` (propagated from `AddRelationship.Parse`) |

## Constraints

- Available in workflow mode only; direct CLI invocation is rejected.
- Package identity comparison uses `SpdxPackage.Same` (by name and download location).
- When enhancing an existing package, the existing package ID is renamed to the
  new ID via `RenameId.Rename` so all existing references remain consistent.
- The `relationships` sequence is optional; omitting it results in no new relationships.
- Variable expansion is applied to all string inputs via `GetMapString`.
