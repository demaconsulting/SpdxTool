# DemaConsulting.SpdxTool Commands Subsystem Design

## Purpose

The Commands subsystem provides the implementations for all CLI subcommands exposed
by DemaConsulting.SpdxTool. Each subcommand corresponds to a discrete SPDX document
operation or workflow step, registered by name in the `CommandsRegistry` and dispatched
by `Program`.

## Architecture

### Command Registry Pattern

All commands are registered in `CommandsRegistry` as `CommandEntry` instances. When
`Program` parses a command name from the CLI arguments, it looks up the corresponding
`Command` implementation in the registry and calls `Execute(Context, string[])`.

### Abstract Base Class

`Command` is the abstract base class for all command implementations. It defines the
`Execute(Context context, string[] args)` interface that every subcommand must implement.
Error handling uses two exception types:

- `CommandUsageException` — thrown when arguments are invalid; triggers usage display
- `CommandErrorException` — thrown when a command fails at runtime; triggers error display

### Individual Commands

| Command Class      | CLI Name            | Purpose                                           |
|--------------------|---------------------|---------------------------------------------------|
| `AddPackage`       | `add-package`       | Add a new package to an SPDX document             |
| `AddRelationship`  | `add-relationship`  | Add a relationship between SPDX elements          |
| `CopyPackage`      | `copy-package`      | Copy a package from one SPDX document to another  |
| `Diagram`          | `diagram`           | Generate a Mermaid diagram from an SPDX document  |
| `FindPackage`      | `find-package`      | Find packages in an SPDX document by criteria     |
| `GetVersion`       | `get-version`       | Retrieve package version from an SPDX document    |
| `Hash`             | `hash`              | Compute or verify file hashes                     |
| `Help`             | `help`              | Display usage/help information                    |
| `Print`            | `print`             | Print a message to the console (workflow step)    |
| `Query`            | `query`             | Query external program output (workflow step)     |
| `RenameId`         | `rename-id`         | Rename an SPDX element ID                         |
| `RunWorkflow`      | `run-workflow`      | Execute a workflow YAML file                      |
| `SetVariable`      | `set-variable`      | Set a workflow variable (workflow step)           |
| `ToMarkdown`       | `to-markdown`       | Convert an SPDX document to Markdown              |
| `UpdatePackage`    | `update-package`    | Update package metadata in an SPDX document       |
| `Validate`         | `validate`          | Validate an SPDX document                         |

## Data Flow

```text
CLI arguments
      │
      ▼
CommandsRegistry.Lookup(commandName)
      │
      ▼
Command.Execute(Context, args)
      │
      ├─► Read SPDX JSON from file system
      │
      ├─► Perform SPDX operation (via SpdxHelpers)
      │
      └─► Write SPDX JSON to file system
              │
              └─► Output results to Context (console/log)
```

## Workflow Execution

The `RunWorkflow` command reads a YAML workflow file and iterates over its steps.
Each step specifies a command name and its arguments. Steps are dispatched back
through `CommandsRegistry`, allowing any registered command to be used as a workflow
step. Variable substitution (`${{ variables.name }}`) is performed on step arguments
before dispatch, using values from the `Context` variable map.

NuGet package workflows are resolved from the local NuGet cache before execution,
enabling versioned and distributable workflow definitions.

## Error Handling

- **Usage errors** (`CommandUsageException`): Display command-specific usage text and
  exit with a non-zero code.
- **Runtime errors** (`CommandErrorException`): Display a descriptive error message and
  exit with a non-zero code.
- **Unhandled exceptions**: Propagate to `Program`, which displays a generic error
  message and exits.

## Design Constraints

- Commands are stateless; all mutable state is carried by the `Context` parameter.
- Commands do not reference each other directly; all cross-command calls go through
  `CommandsRegistry` to maintain loose coupling.
- File paths in command arguments are resolved relative to the current working directory
  using `PathHelpers`.
