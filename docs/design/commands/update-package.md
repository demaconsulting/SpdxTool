# DemaConsulting.SpdxTool update-package Command Design

## Purpose

The `update-package` command updates the metadata of an existing package in an
SPDX document. It is only valid inside a workflow YAML file; direct command-line
invocation is rejected.

## Arguments / Inputs

This command is only valid inside a workflow YAML file:

```yaml

- command: update-package

  inputs:
    spdx: <spdx.json>             # SPDX file name (required)
    package:                      # Package identification and updates
      id: <id>                    # Package ID to update (required)
      name: <name>                # Optional new name
      download: <download-url>    # Optional new download URL
      version: <version>          # Optional new version
      filename: <filename>        # Optional new filename
      supplier: <supplier>        # Optional new supplier
      originator: <originator>    # Optional new originator
      homepage: <homepage>        # Optional new homepage
      copyright: <copyright>      # Optional new copyright text
      summary: <summary>          # Optional new summary
      description: <description>  # Optional new description
      license: <license>          # Optional new license (sets both concluded and declared)
```

## Implementation

1. Reads `spdx` and `package` inputs.
2. Reads `id` from the `package` sub-map.
3. `ParseUpdates` reads each optional field from the map into an

   `updates` dictionary, skipping absent keys.

4. `UpdatePackageInSpdxFile` loads the document, finds the package by ID

   (raises `CommandErrorException` if not found), applies each update by
   setting the corresponding property, and saves the document.

### Supported update keys

`name`, `download`, `version`, `filename`, `supplier`, `originator`, `homepage`,
`copyright`, `summary`, `description`, `license`.
Any other key raises `CommandErrorException`.

When `license` is updated, both `ConcludedLicense` and `DeclaredLicense` are set.

## Error Handling

| Condition | Exception |
| :--- | :--- |
| Invoked from command line (not workflow) | `CommandUsageException` |
| Missing `spdx` input | `YamlException` |
| Missing `package` input | `YamlException` |
| Missing `package.id` input | `YamlException` |
| Package ID not found in document | `CommandErrorException` |
| Invalid update key | `CommandErrorException` |

## Constraints

- Available in workflow mode only; direct CLI invocation raises `CommandUsageException`.
- Only the fields present in the `package` map are updated; absent fields leave

  the existing values unchanged.

- Variable expansion is applied to all string inputs via `GetMapString`.
