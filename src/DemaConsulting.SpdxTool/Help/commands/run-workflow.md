---
title: run-workflow command
summary: Run the steps of a workflow file or URL, with parameters, and return its variables.
tags:
  - reference
see-also:
  - workflow-files
---
`run-workflow` runs the steps of a workflow file; [[workflow-files]]
describes the format and has a sample with runnable commands. On the
command line give a file or an `http://` or `https://` URL:

```text
{{tool}} run-workflow <workflow.yaml> [name=value]... [--verbose]
```

Each `name=value` sets a parameter that the workflow declares; an
undeclared one exits 1 with `parameter <name> not defined`. `--verbose`
prints the workflow's variables after the last step, under `Outputs:`.

As a workflow step it runs another workflow:

```yaml
- command: run-workflow
  inputs:
    file: <workflow.yaml>         # Optional workflow file
    url: <url>                    # Optional workflow url
    nuget: <package:version>      # Optional NuGet package
    integrity: <sha256>           # Optional workflow integrity check
    parameters:
      name: <value>               # Optional workflow parameter
    outputs:
      name: <variable>            # Optional output to save to variable
```

**Source.** Give exactly one of `file` and `url`. With `nuget`, as in
`DemaConsulting.Example:1.0.0`, the package is fetched into the NuGet
cache and `file` is the path of the workflow inside it.
`integrity` is the SHA-256 of the workflow file as hexadecimal text; a
different file exits 1 with `Integrity check of <source> failed`.

**Parameters and outputs.** `parameters` sets declared parameters of the
inner workflow and expands variables of the outer one. Each entry of
`outputs` copies an inner variable (the key) into an outer variable
(the value).
