---
title: to-markdown command
summary: Write a Markdown summary of an SPDX document, with its packages and tools.
tags:
  - reference
see-also:
  - commands/diagram
  - spdx-documents
examples:
  - name: summary
    kind: command
    language: shell
    description: Write the summary of sbom.spdx.json to sbom.md with a title and level-3 headings.
    content: '{{tool}} to-markdown sbom.spdx.json sbom.md "Example SBOM" 3'
---
`to-markdown` writes a Markdown summary of an SPDX document, for example
for a release report. On the command line:

```text
{{tool}} to-markdown <spdx.json> <out.md> [title] [depth]
```

As a workflow step:

```yaml
- command: to-markdown
  inputs:
    spdx: <spdx.json>             # SPDX file name
    markdown: <out.md>            # Output markdown file
    title: <title>                # Optional title
    depth: <depth>                # Optional heading depth
```

The title defaults to `SPDX Document` and the heading level of the title
to 2; the sections are one level deeper. The depth must be 1 or more.
The command that [[#summary]] prints writes `### Example SBOM`.

**Content.** A table of the file name, document name, the numbers of
files, packages, and relationships, the creation time, and the creators,
then a table of name, version, and license for each of these sections:

- `Root Packages`: the packages that the document describes
- `Packages`: the other packages, apart from tools
- `Tools`: packages related as build, development, or test tools

A section without packages is left out.
