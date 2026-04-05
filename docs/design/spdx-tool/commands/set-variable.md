# DemaConsulting.SpdxTool set-variable Command Design

## Purpose

The `set-variable` command sets a workflow variable to a specified value. It is
only valid inside a workflow YAML file; direct command-line invocation is rejected.

## Arguments / Inputs

This command is only valid inside a workflow YAML file:

```yaml
- command: set-variable
  inputs:
    value: <value>                # New value (required)
    output: <variable>            # Variable name to set (required)
```

- `value` — The new value to assign; supports variable expansion.
- `output` — Name of the variable in the workflow's variables map to set.

## Implementation

1. Reads `value` and `output` from the `inputs` map.
2. Applies variable expansion to `value` via `GetMapString`.
3. Assigns `variables[output] = value`.

## Error Handling

| Condition | Exception |
| :--- | :--- |
| Invoked from command line (not workflow) | `CommandUsageException` |
| Missing `value` input | `YamlException` |
| Missing `output` input | `YamlException` |

## Constraints

- Available in workflow mode only; direct CLI invocation raises `CommandUsageException`.
- Variable expansion is applied to `value` before assignment, so the value can
  reference other variables already set in the workflow.
- The `output` variable name is not expanded; it is used literally as the key.
