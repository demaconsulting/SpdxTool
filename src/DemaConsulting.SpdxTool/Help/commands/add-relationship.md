---
title: add-relationship command
summary: Add relationships between elements of an SPDX document.
tags:
  - reference
see-also:
  - spdx-documents
examples:
  - name: add
    kind: command
    language: shell
    description: Record that the library is statically linked into the application, with a comment.
    content: '{{tool}} add-relationship sbom.spdx.json SPDXRef-Library STATIC_LINK SPDXRef-Application "linked statically"'
---
`add-relationship` adds relationships from one element of an SPDX
document to others. On the command line it adds one:

```text
{{tool}} add-relationship <spdx.json> <id> <type> <element> [comment]
```

As a workflow step it adds a list:

```yaml
- command: add-relationship
  inputs:
    spdx: <spdx.json>             # SPDX file name
    id: <id>                      # Element ID
    replace: true                 # Replace existing relationships (default: true)
    relationships:
      - type: <relationship>      # Relationship type
        element: <element>        # Related element
        comment: <comment>        # Optional comment
```

The `type` describes the relationship of `id` to `element`; both are
element IDs, such as `SPDXRef-Library`. The types are those of the SPDX
specification, such as `DESCRIBES`, `CONTAINS`, `DEPENDS_ON`,
`DEPENDENCY_OF`, `STATIC_LINK`, or `BUILD_TOOL_OF`.
The command that [[#add]] prints adds one.

**Replace.** In a workflow, `replace` defaults to `true`: an existing
relationship between the same two elements is removed first, whatever
its type, so the step can change the type. The command line never
replaces; it adds the relationship next to any existing one.
