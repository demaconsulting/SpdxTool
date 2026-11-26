---
title: help command
summary: Print the usage text of a command from a workflow step (workflow only).
tags:
  - reference
see-also:
  - commands
  - command-line
---
`help` as a workflow step prints the usage text of the command named by
`about`:

```yaml
- command: help
  inputs:
    about: <command>              # Command to describe
```

An unknown command name prints `Error: Unknown command: '<command>'`
and exits 1, and a step without `about` exits 1 too.

**On the command line**, `{{tool}} help` as the first argument is the
built-in help that prints these cards; it takes topics and command
names, such as `{{tool}} help <command>`; see [[command-line]]. After an
option, as in `{{tool}} --silent help`, it is this workflow-only command
and exits 1 with `'help' command is only valid in a workflow`.
