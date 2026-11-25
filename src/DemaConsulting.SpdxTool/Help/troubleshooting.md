---
title: Errors and exit codes
summary: What the exit codes mean, and how to fix the common error messages.
aliases:
  - errors
  - problems
  - exit-codes
tags:
  - troubleshooting
order: 8
see-also:
  - command-line
  - commands
examples:
  - name: missing-file
    kind: command
    language: shell
    description: An SPDX document that does not exist. Exits 1.
    content: '{{tool}} validate missing.spdx.json'
    exit: 1
    expect: 'Error: File not found: missing.spdx.json'
  - name: unknown-command
    kind: command
    language: shell
    description: A command that does not exist. Exits 1.
    content: '{{tool}} verify sbom.spdx.json'
    exit: 1
    expect: "Error: Unknown command 'verify'"
  - name: workflow-only
    kind: command
    language: shell
    description: A workflow-only command on the command line. Exits 1.
    content: '{{tool}} add-package'
    exit: 1
    expect: "Error: 'add-package' command is only valid in a workflow"
  - name: bad-depth
    kind: command
    language: shell
    description: A --depth that is not a number. Exits 1.
    content: '{{tool}} --depth abc --validate'
    exit: 1
    expect: "Error: Invalid depth value 'abc': must be an integer"
  - name: unknown-topic
    kind: command
    language: shell
    description: A help topic that does not exist. Exits 2.
    content: '{{tool}} help no-such-topic'
    exit: 2
    expect: "Unknown help card 'no-such-topic'"
---
**Exit codes.**

| Code | Meaning |
| --- | --- |
| 0 | Success. |
| 1 | An error, issues found by `validate`, or a failed self-test. |
| 2 | A wrong `help` argument or an unknown help topic. |

Errors are printed as `Error: ...` and set the exit code. Errors found
during a run are hidden by `--silent` and kept in the `--log` file.
Errors in the options, such as a missing value or a log file that
cannot be created, are found before the run starts: they are always
printed and never reach the log file.

**Common messages.**

- `Unknown command`: check the spelling against [[commands]]. Options
  go before the command; an unknown option is read as the command.
  The command that [[#unknown-command]] prints shows the error.
- `File not found`: the SPDX document does not exist. A relative path
  is relative to the current folder, or to the package root in
  `dotnet pack`. The command that [[#missing-file]] prints shows it.
- `command is only valid in a workflow`: run the command as a step of
  a workflow file; see [[workflow-files]].
  The command that [[#workflow-only]] prints shows the error.
- `command missing arguments` and similar: the usage text follows; the
  card of the command shows its arguments.
- `Missing depth argument`, `Invalid depth value`: an option needs a
  value; see [[command-line]]. [[#bad-depth]] shows one.
- `Found <n> Issues`: `validate` found problems in the document, listed
  above the message; see [[spdx-documents]].
- `Package not found` or `Multiple packages found`: the criteria of
  `find-package` or `get-version` must match exactly one package.
- `parameter <name> not defined`: declare the parameter in the
  workflow's `parameters` before setting it on the command line.
- `Unknown help card`: the topic does not exist. The message lists the
  topics. The command that [[#unknown-topic]] prints shows the error.

**Unexpected errors.** Some failures, such as a file that is not valid
JSON or an unknown relationship type, print the full exception text
instead of `Error: ...`. They also exit 1.
