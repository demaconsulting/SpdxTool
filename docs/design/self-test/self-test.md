# DemaConsulting.SpdxTool SelfTest Subsystem Design

## Purpose

The SelfTest subsystem implements the `--validate` self-test suite for
DemaConsulting.SpdxTool. It exercises every command against embedded SPDX fixtures
to verify that the tool is functioning correctly after installation or deployment,
without requiring external tools or network access.

## Architecture

### Orchestrator

`Validate.cs` is the entry point for self-validation. When `Program` detects the
`--validate` flag, it calls `Validate.Run(Context)` instead of dispatching to a
command. `Validate.Run` creates a temporary working directory, copies embedded SPDX
fixture files into it, and then invokes each `Validate*` step class in sequence.

### Validation Steps

Each `Validate*.cs` class tests one or more commands by invoking them through the
`CommandRegistry` against the fixture files. Results are collected as pass/fail entries
and reported to the `Context` output stream.

| Step Class                  | Commands Exercised           |
|-----------------------------|------------------------------|
| `ValidateBasic`             | version, help, silent, log   |
| `ValidateAddPackage`        | add-package, run-workflow    |
| `ValidateAddRelationship`   | add-relationship             |
| `ValidateCopyPackage`       | copy-package                 |
| `ValidateDiagram`           | diagram                      |
| `ValidateFindPackage`       | find-package                 |
| `ValidateGetVersion`        | get-version                  |
| `ValidateHash`              | hash                         |
| `ValidateNtia`              | validate (NTIA check)        |
| `ValidateQuery`             | query                        |
| `ValidateRenameId`          | rename-id                    |
| `ValidateRunNuGetWorkflow`  | run-workflow (NuGet)         |
| `ValidateToMarkdown`        | to-markdown                  |
| `ValidateUpdatePackage`     | update-package               |

## Result Reporting

Self-validation results are reported in three ways:

1. **Console output**: Pass/fail status for each test is written to the `Context`
   output stream, with optional depth control (`--depth`) to show hierarchical detail.
2. **TRX output**: When the `--result` flag specifies a `.trx` file path, results are
   written in Visual Studio TRX format for integration with CI/CD test reporting tools.
3. **JUnit XML output**: When the `--result` flag specifies a `.xml` file path, results
   are written in JUnit XML format for integration with CI/CD systems that consume
   JUnit reports.

## Data Flow

```text
Program.cs detects --validate flag
      │
      ▼
Validate.Run(Context)
      │
      ├─► Create temporary working directory
      │
      ├─► Copy embedded SPDX fixture files
      │
      ├─► For each Validate* step:
      │       │
      │       ├─► Invoke command(s) via CommandRegistry
      │       │
      │       └─► Record pass/fail result
      │
      └─► Report results
              ├─► Console (with optional depth)
              ├─► TRX file (if --result *.trx)
              └─► JUnit XML file (if --result *.xml)
```

## Design Constraints

- Self-validation is entirely in-process; no external tools or network access are needed.
- Fixture files are embedded as resources to ensure they are always available.
- The temporary working directory is cleaned up after validation completes.
- Self-validation bypasses normal command dispatch; the `--validate` flag is processed
  before command name lookup in `Program`.
