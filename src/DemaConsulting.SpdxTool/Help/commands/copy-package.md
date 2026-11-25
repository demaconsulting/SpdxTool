---
title: copy-package command
summary: Copy a package from one SPDX document to another.
tags:
  - reference
see-also:
  - commands/add-relationship
  - spdx-documents
examples:
  - name: copy
    kind: command
    language: shell
    description: Copy the package SPDXRef-Json of lib.spdx.json into sbom.spdx.json.
    content: '{{tool}} copy-package lib.spdx.json sbom.spdx.json SPDXRef-Json'
---
`copy-package` copies a package from one SPDX document to another, for
example to merge the SBOM of a library into the SBOM of an application.
On the command line:

```text
{{tool}} copy-package <from.spdx.json> <to.spdx.json> <package> [recursive] [files]
```

As a workflow step:

```yaml
- command: copy-package
  inputs:
    from: <from.spdx.json>        # Source SPDX file name
    to: <to.spdx.json>            # Destination SPDX file name
    package: <package>            # Package ID
    recursive: true               # Optional recursive flag
    files: true                   # Optional copy-files flag
    relationships:                # Optional relationships
      - type: <relationship>      # Relationship type
        element: <element>        # Related element
        comment: <comment>        # Optional comment
```

`<package>` is the ID of the package in the source document. Only the
destination document is changed. The command that [[#copy]] prints
copies one package.

**Options.** `recursive` also copies the child packages, those that a
relationship such as `CONTAINS` or `DEPENDS_ON` places below the
package, and their children, with those relationships. `files` also
copies the files of the package. Any other option word is an error.

**Same package.** When the destination already has a package with the
same name and version, that package is enhanced instead of duplicated.

**Relationships.** In a workflow, `relationships` relate the copied
package to elements of the destination, as in `add-package`. On the
command line, add them afterward with `add-relationship`.
