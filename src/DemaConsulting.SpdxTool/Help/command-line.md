---
title: Command-line options
summary: The global options of the tool, and the order in which a run handles them.
aliases:
  - options
  - flags
  - usage
tags:
  - reference
order: 2
see-also:
  - commands
  - troubleshooting
examples:
  - name: usage
    kind: command
    language: shell
    description: Print the usage text. It ends with a pointer to this help.
    content: '{{tool}} --help'
    expect: "Run '{{tool}} help' for topics and examples."
  - name: silent-log
    kind: command
    language: shell
    description: Hide the console output and keep it in a log file.
    content: '{{tool}} --silent --log spdx-tool.log validate sbom.spdx.json'
  - name: missing-value
    kind: command
    language: shell
    description: An option without its value is an error. Exits 1.
    content: '{{tool}} --depth'
    exit: 1
    expect: 'Error: Missing depth argument'
---
Run the tool as `{{tool}} [options] <command> [arguments]`. The options
come first; the first argument that is not an option is the command, and
every argument after it, even one that starts with `-`, goes to the
command. The commands are listed in [[commands]].

| Option | Meaning |
| --- | --- |
| `-v`, `--version` | Print only the version. |
| `-?`, `-h`, `--help` | Print the usage text. |
| `-s`, `--silent` | Hide console output. Errors still set exit code 1. |
| `-l`, `--log <file>` | Also write the output to a file. |
| `--validate` | Run the self-tests; see [[self-validation]]. |
| `-r`, `--result <file>` | Write the self-test results as `.trx` or `.xml`. |
| `--depth <level>` | Heading level of the self-test report; default 1. |

**Order.** `--version`, when all arguments are valid, prints only the
version and skips everything else. Without it, the tool prints a banner
with its name and version, then handles the first of `--help`,
`--validate`, or the command. Without a command it prints
`Error: Missing arguments` and the usage text.
The command that [[#silent-log]] prints writes its output to the log file
only.

**Errors.** An option without its value, a `--depth` that is not a
non-negative integer, or a log file that cannot be created prints
`Error: ...` and exits 1, even with `--silent`, and never reaches the log
file. The command that [[#missing-value]] prints shows this. An unknown
option counts as the command, so it is reported as `Unknown command`.

**Help.** `help` is a command, not an option, and must be the first
argument. `--silent` and `--log` do not apply to it, and an unknown topic
exits 2. After an option, as in `{{tool}} --silent help`, `help` is the
workflow-only command and exits 1; see [[commands/help]].
