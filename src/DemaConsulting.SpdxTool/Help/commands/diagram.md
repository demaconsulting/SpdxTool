---
title: diagram command
summary: Write a Mermaid entity-relationship diagram of the packages of an SPDX document.
tags:
  - reference
see-also:
  - commands/to-markdown
  - spdx-documents
examples:
  - name: diagram
    kind: command
    language: shell
    description: Write the package diagram of sbom.spdx.json to sbom.mmd.
    content: '{{tool}} diagram sbom.spdx.json sbom.mmd'
  - name: tools
    kind: command
    language: shell
    description: Include the build, development, and test tools in the diagram.
    content: '{{tool}} diagram sbom.spdx.json sbom.mmd tools'
---
`diagram` writes a Mermaid `erDiagram` of the relationships between the
packages of an SPDX document to a file. On the command line:

```text
{{tool}} diagram <spdx.json> <mermaid.txt> [tools]
```

As a workflow step:

```yaml
- command: diagram
  inputs:
    spdx: <spdx.json>             # SPDX file name
    mermaid: <mermaid.txt>        # Mermaid file name
    tools: true                   # Optionally include tools
```

Each line of the diagram joins two packages, shown as `name / version`,
and is labeled with the relationship type, with the containing or
depending package first. Relationships to elements that are not
packages, such as the document itself, are left out.

The command that [[#diagram]] prints leaves out `BUILD_TOOL_OF`,
`DEV_TOOL_OF`, and `TEST_TOOL_OF` relationships; the option `tools`, as
in the command that [[#tools]] prints, keeps them. The diagram goes only
to the file; nothing of it is printed.
