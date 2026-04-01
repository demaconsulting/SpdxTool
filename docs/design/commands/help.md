# DemaConsulting.SpdxTool help Command Design

## Purpose

The `help` command displays extended help information about a specific command.
It is available from the command-line and from workflow YAML files.

## Arguments / Inputs

### Command-line usage

```text
spdx-tool help <command>
```

- `command` — Name of the command to display help for.

### Workflow YAML usage

```yaml
- command: help
  inputs:
    about: <command>              # Command to display help for (required)
```

## Implementation

1. Reads the command name from the single CLI argument or the `about` input.
2. Looks up the command in `CommandsRegistry.Commands`.
3. If found, iterates over `entry.Details` and writes each line to the context.
4. If not found, raises `CommandUsageException`.

## Error Handling

| Condition | Exception |
| :--- | :--- |
| Not exactly 1 CLI argument | `CommandUsageException` |
| Missing `about` input (workflow) | `YamlException` |
| Unknown command name | `CommandUsageException` |

## Constraints

- The command name must exactly match the registered command name (case-sensitive).
- Help text is taken directly from the `CommandEntry.Details` array defined by
  each command's `Entry` static field.
- Variable expansion is applied to string inputs via `GetMapString`.
