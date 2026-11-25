---
title: find-package command
summary: Find the ID of the one package of an SPDX document that matches search criteria.
tags:
  - reference
see-also:
  - commands/get-version
  - workflow-files
examples:
  - name: by-name
    kind: command
    language: shell
    description: Print the ID of the package named Example Library.
    content: '{{tool}} find-package sbom.spdx.json "name=Example Library"'
    expect: 'SPDXRef-Library'
  - name: wildcard
    kind: command
    language: shell
    description: Match with a wildcard; the pattern must still match exactly one package.
    content: '{{tool}} find-package sbom.spdx.json "name=Example Lib*"'
    expect: 'SPDXRef-Library'
---
`find-package` finds the package that matches every given criterion and
prints its ID. On the command line give at least one criterion:

```text
{{tool}} find-package <spdx.json> <criteria>...
```

The criteria are `key=value` words with these keys:

- `id=<id>`: the package ID
- `name=<name>`: the package name
- `version=<version>`: the package version
- `filename=<filename>`: the package file name
- `download=<url>`: the package download location

As a workflow step, the criteria are inputs and the ID is saved in the
variable named by `output`:

```yaml
- command: find-package
  inputs:
    output: <variable>            # Output variable for package ID
    spdx: <spdx.json>             # SPDX file name
    id: <id>                      # Optional package ID
    name: <name>                  # Optional package name
    version: <version>            # Optional package version
    filename: <filename>          # Optional package filename
    download: <url>               # Optional package download URL
```

**Matching.** A value may use the wildcards `*` (any text) and `?` (one
character), and is compared without regard to case, as the command that
[[#wildcard]] prints shows. Exactly one package must match: no match, or
more than one, is an error that lists the criteria and exits 1. Quote a
criterion that contains a space, as in [[#by-name]].
