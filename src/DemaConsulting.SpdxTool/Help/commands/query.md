---
title: query command
summary: Run a program and print a value that a regular expression captures from its output.
tags:
  - reference
see-also:
  - workflow-files
examples:
  - name: dotnet-version
    kind: command
    language: shell
    description: Print the version of the .NET SDK that `dotnet --version` reports.
    content: '{{tool}} query "(?<value>[0-9]+[.][0-9]+[.][0-9]+)" dotnet --version'
---
`query` runs a program, searches its output for a regular expression,
and prints the text of the expression's `value` group. A workflow uses
it to put a tool version into a variable. On the command line:

```text
{{tool}} query <pattern> <program> [args]...
```

The command that [[#dotnet-version]] prints shows one. As a workflow
step the value is saved in the variable named by `output`:

```yaml
- command: query
  inputs:
    output: <variable>
    pattern: <regex with 'value' capture>
    program: <program>
    arguments:
      - <argument>
      - <argument>
```

**Matching.** Standard output and standard error are searched line by
line, and the first line where the `value` group captures text wins.
The exit code of the program is ignored.

**Errors.** A pattern without a `(?<value>...)` group, or one that is not
a valid regular expression, is a usage error. A program that cannot be
started, or output without a match, exits 1.
