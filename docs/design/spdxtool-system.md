# DemaConsulting.SpdxTool System Design

## System Overview

DemaConsulting.SpdxTool is one of two systems in this repository. It is a .NET tool
distributed as a NuGet package that exposes a command-line interface for creating,
validating, and manipulating SPDX documents. The companion system,
`DemaConsulting.SpdxTool.Targets`, integrates SPDX decoration into the `dotnet pack`
workflow via MSBuild targets.

## System Architecture

The system follows a command-pattern architecture where a central registry dispatches
CLI subcommands to dedicated command classes. Global flags are processed by `Program`
before dispatch, with `--validate` redirecting execution to the SelfTest subsystem
rather than a regular command.

### Major Components

- **Program** — parses the CLI arguments, initializes a `Context`, and dispatches to
  `CommandRegistry`.
- **Context** — carries the runtime execution state: console/log output streams, the
  silent flag, and the mutable variables map used by workflow commands.
- **CommandRegistry** — maintains the map of command names to `Command` implementations
  and performs command lookup and dispatch.
- **Commands subsystem** — contains one `Command`-derived class per supported CLI
  subcommand (e.g., `AddPackage`, `RunWorkflow`, `Validate`).
- **SelfTest subsystem** — contains the `--validate` self-test suite that exercises
  all commands against embedded SPDX fixtures.
- **Spdx unit group** — provides SPDX-domain helpers (`SpdxHelpers`) and the
  `RelationshipDirection` enumeration consumed throughout the commands.
- **Utility unit group** — provides cross-cutting helpers: `PathHelpers` for file path
  resolution and `Wildcard` for glob pattern matching.
- **Targets subsystem** — MSBuild `.targets` files that invoke `spdx-tool run-workflow`
  during `dotnet pack`.

## External Interfaces

### Command-Line Interface

The tool is invoked as `spdx-tool <command> [options]` where `<command>` is one of the
registered command names. Global flags (`--silent`, `--log`, `--validate`, etc.) are
parsed by `Program` before command dispatch. The `--validate` flag bypasses command
dispatch and runs the self-validation suite instead.

### File System

Commands read and write SPDX JSON documents from/to paths supplied as command arguments.
Workflow files are YAML files that specify a sequence of command invocations. NuGet
package-based workflows are resolved from the local NuGet cache.

### NuGet

The `run-workflow` command supports workflow files embedded in NuGet packages. It
resolves the package from the local NuGet cache using the package ID and version
supplied in the workflow step.

### MSBuild Integration

`DemaConsulting.SpdxTool.Targets` injects a `DecorateNuGetSbom` target into the build
pipeline. This target runs after `dotnet pack` and conditionally invokes
`spdx-tool run-workflow` with a user-supplied workflow file path.

## Data Flow

```text
CLI Input
    │
    ▼
Program.cs  ──────────────────────────────────────┐
    │  parse global flags + command name           │
    ▼                                              │
Context.cs  (output, log, variables)               │
    │                                              │ --validate flag
    ▼                                              ▼
CommandRegistry ──► Command.Execute()     SelfTest.Validate.Run()
                         │                        │
                         ▼                        ▼
                   SPDX document I/O        SelfTest.*
                   (read/write JSON)        (exercise each command)
```

## Design Constraints

- **Cross-platform**: The tool must run on Windows, Linux, and macOS using .NET 8, 9,
  and 10. All file I/O uses `Path` APIs to maintain portability.
- **No global state**: All mutable state is encapsulated in `Context` and passed
  explicitly; commands are stateless.
- **Workflow isolation**: Each workflow step executes in the same `Context` instance,
  allowing variables set in one step to be consumed in subsequent steps.
- **Self-contained validation**: The `--validate` flag runs the entire command suite
  using only in-process calls. The `ValidateQuery` step spawns `dotnet` as an external
  process, and `ValidateRunNuGetWorkflow` may restore a NuGet package on a cache miss
  (requiring network access). All other steps are fully in-process.

## Integration Patterns

- **Command pattern**: Each CLI subcommand is a self-contained `Command` class with
  `Execute(Context, string[])` semantics, registered by name in `CommandRegistry`.
- **Variable substitution**: Workflow YAML supports `${{ variables.name }}` tokens that
  are replaced at runtime from the `Context` variable map.
- **Source-filter evidence**: ReqStream source filters (`windows@`, `ubuntu@`, `net8.0@`)
  restrict test evidence to specific CI/CD matrix legs, ensuring platform requirements
  are validated on the correct runtime environment.
