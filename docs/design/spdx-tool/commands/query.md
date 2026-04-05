# DemaConsulting.SpdxTool query Command Design

## Purpose

The `query` command executes an external program, captures its combined stdout and
stderr output, and extracts a value by matching lines against a regular expression
with a named capture group `value`. The captured value is written to the console
or stored in a workflow variable.

## Arguments / Inputs

### Command-line usage

```text
spdx-tool query <pattern> <program> [args]
```

- `pattern` — Regular expression with a `(?<value>...)` capture group.
- `program` — External program to execute.
- `args` — Zero or more arguments passed to the program.

### Workflow YAML usage

```yaml
- command: query
  inputs:
    output: <variable>            # Output variable (required)
    pattern: <regex>              # Regex with 'value' capture group (required)
    program: <program>            # Program to execute (required)
    arguments:                    # Optional argument list
    - <argument>
    - <argument>
```

## Implementation

1. Validates that `pattern` contains a `value` capture group; raises
   `CommandUsageException` if not.
2. Constructs a `ProcessStartInfo` with `RedirectStandardOutput`, `RedirectStandardError`,
   and `UseShellExecute = false`.
3. Starts the process; raises `CommandErrorException` if it cannot start.
4. Reads combined stdout + stderr, waits for exit.
5. Splits output into lines, trims each, and applies the regex. The first non-empty
   `value` capture group match is returned.
6. CLI path: writes the result to console.
7. Workflow path: stores the result in `variables[output]`.

## Error Handling

| Condition | Exception |
| :--- | :--- |
| Fewer than 2 CLI arguments | `CommandUsageException` |
| Missing `output` input (workflow) | `YamlException` |
| Missing `pattern` input (workflow) | `YamlException` |
| Missing `program` input (workflow) | `YamlException` |
| Pattern missing `value` capture group | `CommandUsageException` |
| Program cannot be started | `CommandErrorException` |
| Pattern not found in program output | `CommandErrorException` |

## Constraints

- The regex is compiled with a 100 ms timeout per match to prevent ReDoS.
- Arguments in workflow mode undergo variable expansion.
- The match is performed per-line (not across multiple lines).
- Only the first matching `value` capture is used.
