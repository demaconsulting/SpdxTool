# DemaConsulting.SpdxTool print Command Design

## Purpose

The `print` command outputs one or more lines of text to the console. Variable
expansion is applied to each line so workflow variable values can be embedded in
the output. It is available from the command-line and from workflow YAML files.

## Arguments / Inputs

### Command-line usage

```text
spdx-tool print <text> [text] ...
```

Each positional argument is printed as a separate line.

### Workflow YAML usage

```yaml
- command: print
  inputs:
    text:
    - Some text to print
    - The value of variable is ${{ variable }}
```

The `text` input is a YAML sequence; each entry is printed as a line.
Variable expansion (`${{ variable }}`) is applied to each entry.

## Implementation

1. CLI path: iterates `args` and calls `context.WriteLine` for each argument.
2. Workflow path: reads the `text` sequence from `inputs`. Iterates each sequence
   index using `GetSequenceString` (which applies variable expansion) and calls
   `context.WriteLine`.

## Error Handling

| Condition | Exception |
| :--- | :--- |
| Missing `text` input (workflow) | `YamlException` |

No error is raised if `text` is an empty sequence (nothing is printed).

## Constraints

- Variable expansion uses the standard `${{ variable }}` syntax via `GetSequenceString`.
- CLI path accepts zero or more arguments (zero arguments prints nothing).
- The `text` sequence in workflow mode is required; the key must be present.
