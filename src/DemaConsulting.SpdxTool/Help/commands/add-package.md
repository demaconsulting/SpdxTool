---
title: add-package command
summary: Add a package to an SPDX document, or enhance the same package (workflow only).
tags:
  - reference
see-also:
  - commands/update-package
  - workflow-files
---
`add-package` adds a package to an SPDX document. It runs only as a
workflow step:

```yaml
- command: add-package
  inputs:
    spdx: <spdx.json>             # SPDX file name
    package:                      # New package information
      id: <id>                    # New package ID
      name: <name>                # New package name
      download: <download-url>    # New package download URL
      version: <version>          # Optional package version
      filename: <filename>        # Optional package filename
      supplier: <supplier>        # Optional package supplier
      originator: <originator>    # Optional package originator
      homepage: <homepage>        # Optional package homepage
      copyright: <copyright>      # Optional package copyright
      summary: <summary>          # Optional package summary
      description: <description>  # Optional package description
      license: <license>          # Optional package license
      purl: <package-url>         # Optional package purl
      cpe23: <cpe-identifier>     # Optional package cpe23
    relationships:                # Optional relationships
      - type: <relationship>      # Relationship type
        element: <element>        # Related element
        comment: <comment>        # Optional comment
```

**Package.** `id`, `name`, and `download` are required. `license` sets
both the concluded and the declared license; it and `copyright` default
to `NOASSERTION`. `purl` and `cpe23` add external references.

**Same package.** When the document already has a package with the same
name and version, that package is enhanced with the new values and its
ID is renamed to `id`, together with every reference to it.

**Relationships.** Each `type` describes the new package's relationship
to `element`, an ID in the document. The types are those of the SPDX
specification, such as `DESCRIBES`, `CONTAINS`, `DEPENDENCY_OF`, or
`BUILD_TOOL_OF`.

The sample workflow in [[workflow-files]] adds a build tool this way.
On the command line the command exits 1 with
`'add-package' command is only valid in a workflow`.
