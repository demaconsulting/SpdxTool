---
title: rename-id command
summary: Rename an element ID in an SPDX document, and every reference to it.
tags:
  - reference
see-also:
  - spdx-documents
examples:
  - name: rename
    kind: command
    language: shell
    description: Rename the package ID SPDXRef-Library to SPDXRef-Lib.
    content: '{{tool}} rename-id sbom.spdx.json SPDXRef-Library SPDXRef-Lib'
---
`rename-id` gives an element of an SPDX document a new ID and updates
every reference to it: in relationships, in the file lists of packages,
in snippets, and in the list of elements that the document describes.
On the command line it takes exactly three arguments:

```text
{{tool}} rename-id <spdx.json> <old-id> <new-id>
```

As a workflow step:

```yaml
- command: rename-id
  inputs:
    spdx: <spdx.json>             # SPDX file name
    old: <old-id>                 # Old element ID
    new: <new-id>                 # New element ID
```

The command that [[#rename]] prints renames one package.

**Rules.** Neither ID may be empty or `SPDXRef-DOCUMENT`, which names
the document itself; that is a usage error. The new ID must not already
belong to a package, file, or snippet: `Element ID <new-id> is already
used` exits 1.
