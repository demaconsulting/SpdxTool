---
title: get-version command
summary: Print the version of the one package of an SPDX document that matches search criteria.
tags:
  - reference
see-also:
  - commands/find-package
  - workflow-files
examples:
  - name: by-id
    kind: command
    language: shell
    description: Print the version of the package SPDXRef-Application.
    content: '{{tool}} get-version sbom.spdx.json id=SPDXRef-Application'
    expect: '1.0.0'
---
`get-version` finds a package like `find-package` and prints its
version, or an empty line when the package has none. On the command
line give at least one criterion:

```text
{{tool}} get-version <spdx.json> <criteria>...
```

The criteria are the `key=value` words of `find-package`: `id`, `name`,
`version`, `filename`, and `download`, with the same wildcards, and
exactly one package must match; see [[commands/find-package]]. The
command that [[#by-id]] prints shows one.

As a workflow step, the criteria are inputs and the version is saved in
the variable named by `output`:

```yaml
- command: get-version
  inputs:
    output: <variable>            # Output variable
    spdx: <spdx.json>             # SPDX file name
    id: <id>                      # Optional package ID
    name: <name>                  # Optional package name
    version: <version>            # Optional package version
    filename: <filename>          # Optional package filename
    download: <url>               # Optional package download URL
```
