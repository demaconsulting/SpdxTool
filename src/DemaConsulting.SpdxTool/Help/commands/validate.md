---
title: validate command
summary: Check an SPDX document against the SPDX rules, and optionally the NTIA minimum elements.
tags:
  - reference
see-also:
  - spdx-documents
  - troubleshooting
examples:
  - name: validate
    kind: command
    language: shell
    description: Check the document against the SPDX rules. Exits 0 when it has no issues.
    content: '{{tool}} validate sbom.spdx.json'
  - name: ntia
    kind: command
    language: shell
    description: Also check the NTIA minimum elements; the example packages have no supplier. Exits 1.
    content: '{{tool}} validate sbom.spdx.json ntia'
    exit: 1
    expect: 'Found 2 Issues in sbom.spdx.json'
---
`validate` checks an SPDX document and prints one line for each issue it
finds. On the command line:

```text
{{tool}} validate <spdx.json> [ntia]
```

As a workflow step:

```yaml
- command: validate
  inputs:
    spdx: <spdx.json>             # SPDX file name
    ntia: true                    # Optional NTIA checking
```

Without issues the command prints nothing more and exits 0, as the
command that [[#validate]] prints shows. With issues it ends with
`Error: Found <n> Issues in <spdx.json>` and exits 1, which fails a
workflow at that step.

**NTIA.** `ntia` also checks the minimum elements of an SBOM that the
NTIA defines, such as a supplier for every package. On the command line
the word must be lowercase `ntia`; any other word is ignored. In a
workflow, `ntia: true` in any case turns it on. The command that
[[#ntia]] prints finds the missing suppliers of the example SBOM.
