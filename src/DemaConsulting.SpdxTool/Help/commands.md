---
title: Commands
summary: The commands of the tool, which run from the command line or as workflow steps.
aliases:
  - command
tags:
  - reference
order: 3
see-also:
  - command-line
  - workflow-files
  - spdx-documents
examples:
  - name: card
    kind: command
    language: shell
    description: Print the card of one command, here to-markdown.
    content: '{{tool}} help to-markdown'
    expect: 'to-markdown <spdx.json> <out.md> [title] [depth]'
---
Run a command as `{{tool}} [options] <command> [arguments]`, or as a step
of a workflow file; see [[workflow-files]]. Each command has its own card,
listed below. Print one with `{{tool}} help <command>`, as the command
that [[#card]] prints shows; `{{tool}} help commands <command>` prints
the same card.

**Workflow only.** `add-package`, `update-package`, `set-variable`, and
`help` run only as workflow steps. On the command line they print
`Error: '<command>' command is only valid in a workflow` and exit 1.
Every other command runs both ways.

**Documents.** Commands that change an SPDX document save it in place;
see [[spdx-documents]].

**Errors.** A wrong number or form of arguments, or a missing input file,
prints `Error: ...` and the usage text. An error found while the command
runs prints only `Error: ...`. Both exit 1; see [[troubleshooting]].
