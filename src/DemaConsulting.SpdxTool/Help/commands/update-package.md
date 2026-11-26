---
title: update-package command
summary: Change fields of an existing package of an SPDX document (workflow only).
tags:
  - reference
see-also:
  - commands/add-package
  - workflow-files
---
`update-package` changes fields of a package that the SPDX document
already has, found by its ID. It runs only as a workflow step:

```yaml
- command: update-package
  inputs:
    spdx: <spdx.json>             # SPDX file name
    package:                      # Package information
      id: <id>                    # Package ID
      name: <name>                # Optional new package name
      download: <download-url>    # Optional new package download URL
      version: <version>          # Optional new package version
      filename: <filename>        # Optional new package filename
      supplier: <supplier>        # Optional new package supplier
      originator: <originator>    # Optional new package originator
      homepage: <homepage>        # Optional new package homepage
      copyright: <copyright>      # Optional new package copyright
      summary: <summary>          # Optional new package summary
      description: <description>  # Optional new package description
      license: <license>          # Optional new package license
```

Only the given fields change. `license` sets both the concluded and the
declared license. A value such as `Organization: Example Corp` contains
`: `, so quote it in YAML.

**Errors.** A package ID that the document does not have exits 1 with
`Package '<id>' not found in <spdx.json>`, and any other key exits 1
with `Invalid package update key '<key>'`; neither changes the file.

The sample workflow in [[workflow-files]] sets a version and a supplier
this way. On the command line the command exits 1 with
`'update-package' command is only valid in a workflow`.
