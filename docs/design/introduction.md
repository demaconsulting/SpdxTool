# DemaConsulting.SpdxTool Design Introduction

## Purpose

This document introduces the design of two related but independent systems in this
repository:

- **DemaConsulting.SpdxTool** — a .NET tool for creating, validating, and
  manipulating SPDX (Software Package Data Exchange) documents.
- **DemaConsulting.SpdxTool.Targets** — an MSBuild targets extension that integrates
  SPDX document decoration into the standard `dotnet pack` build workflow.

This document serves as the entry point for design documentation supporting formal
code review, compliance evidence, and maintenance activities.

## Scope

This design documentation covers both systems and all their constituent subsystems
and units. It applies to all source code under `src/`. Third-party (OTS) components
are referenced but not designed in detail.

## Software Structure

### DemaConsulting.SpdxTool (System)

```text
DemaConsulting.SpdxTool (System)
├── Commands (Subsystem)
│   ├── AddPackage.cs (Unit)
│   ├── AddRelationship.cs (Unit)
│   ├── Command.cs (Unit)
│   ├── CommandEntry.cs (Unit)
│   ├── CommandErrorException.cs (Unit)
│   ├── CommandRegistry.cs (Unit)
│   ├── CommandUsageException.cs (Unit)
│   ├── CopyPackage.cs (Unit)
│   ├── Diagram.cs (Unit)
│   ├── FindPackage.cs (Unit)
│   ├── GetVersion.cs (Unit)
│   ├── Hash.cs (Unit)
│   ├── Help.cs (Unit)
│   ├── Print.cs (Unit)
│   ├── Query.cs (Unit)
│   ├── RenameId.cs (Unit)
│   ├── RunWorkflow.cs (Unit)
│   ├── SetVariable.cs (Unit)
│   ├── ToMarkdown.cs (Unit)
│   ├── UpdatePackage.cs (Unit)
│   └── Validate.cs (Unit)
├── SelfTest (Subsystem)
│   ├── Validate.cs (Unit)
│   ├── ValidateAddPackage.cs (Unit)
│   ├── ValidateAddRelationship.cs (Unit)
│   ├── ValidateBasic.cs (Unit)
│   ├── ValidateCopyPackage.cs (Unit)
│   ├── ValidateDiagram.cs (Unit)
│   ├── ValidateFindPackage.cs (Unit)
│   ├── ValidateGetVersion.cs (Unit)
│   ├── ValidateHash.cs (Unit)
│   ├── ValidateNtia.cs (Unit)
│   ├── ValidateQuery.cs (Unit)
│   ├── ValidateRenameId.cs (Unit)
│   ├── ValidateRunNuGetWorkflow.cs (Unit)
│   ├── ValidateToMarkdown.cs (Unit)
│   └── ValidateUpdatePackage.cs (Unit)
├── Spdx (Unit Group)
│   ├── RelationshipDirection.cs (Unit)
│   └── SpdxHelpers.cs (Unit)
├── Utility (Subsystem)
│   ├── PathHelpers.cs (Unit)
│   └── Wildcard.cs (Unit)
├── Context.cs (Unit)
└── Program.cs (Unit)
```

### DemaConsulting.SpdxTool.Targets (System)

```text
DemaConsulting.SpdxTool.Targets (System)
├── build/DemaConsulting.SpdxTool.Targets.targets  (Unit)
└── buildMultiTargeting/DemaConsulting.SpdxTool.Targets.targets  (Unit)
```

## Folder Layout

```text
src/DemaConsulting.SpdxTool/
├── Commands/
│   ├── AddPackage.cs               — add-package command implementation
│   ├── AddRelationship.cs          — add-relationship command implementation
│   ├── Command.cs                  — abstract base class for all commands
│   ├── CommandEntry.cs             — command entry/dispatch logic
│   ├── CommandErrorException.cs    — exception for command errors
│   ├── CommandRegistry.cs          — registry of all available commands
│   ├── CommandUsageException.cs    — exception for command usage errors
│   ├── CopyPackage.cs              — copy-package command implementation
│   ├── Diagram.cs                  — diagram command implementation
│   ├── FindPackage.cs              — find-package command implementation
│   ├── GetVersion.cs               — get-version command implementation
│   ├── Hash.cs                     — hash command implementation
│   ├── Help.cs                     — help command implementation
│   ├── Print.cs                    — print command implementation
│   ├── Query.cs                    — query command implementation
│   ├── RenameId.cs                 — rename-id command implementation
│   ├── RunWorkflow.cs              — run-workflow command implementation
│   ├── SetVariable.cs              — set-variable command implementation
│   ├── ToMarkdown.cs               — to-markdown command implementation
│   ├── UpdatePackage.cs            — update-package command implementation
│   └── Validate.cs                 — validate command implementation
├── SelfTest/
│   ├── Validate.cs                 — self-test orchestrator
│   ├── ValidateAddPackage.cs       — validates add-package command
│   ├── ValidateAddRelationship.cs  — validates add-relationship command
│   ├── ValidateBasic.cs            — validates basic tool functionality
│   ├── ValidateCopyPackage.cs      — validates copy-package command
│   ├── ValidateDiagram.cs          — validates diagram command
│   ├── ValidateFindPackage.cs      — validates find-package command
│   ├── ValidateGetVersion.cs       — validates get-version command
│   ├── ValidateHash.cs             — validates hash command
│   ├── ValidateNtia.cs             — validates NTIA validation command
│   ├── ValidateQuery.cs            — validates query command
│   ├── ValidateRenameId.cs         — validates rename-id command
│   ├── ValidateRunNuGetWorkflow.cs — validates NuGet workflow execution
│   ├── ValidateToMarkdown.cs       — validates to-markdown command
│   └── ValidateUpdatePackage.cs    — validates update-package command
├── Spdx/
│   ├── RelationshipDirection.cs    — SPDX relationship direction enumeration
│   └── SpdxHelpers.cs              — SPDX document utility helpers
├── Utility/
│   ├── PathHelpers.cs              — file path utility helpers
│   └── Wildcard.cs                 — wildcard pattern matching
├── Context.cs                      — execution context (output, logging)
└── Program.cs                      — tool entry point and CLI parsing

src/DemaConsulting.SpdxTool.Targets/
├── build/
│   └── DemaConsulting.SpdxTool.Targets.targets  — single-TFM MSBuild targets
└── buildMultiTargeting/
    └── DemaConsulting.SpdxTool.Targets.targets  — multi-TFM MSBuild targets
```

## Per-Unit Design Documentation

Per-unit design documentation is maintained for Commands and SelfTest subsystem units:

- `docs/design/spdx-tool/commands/<command>.md` — design doc for each command unit
- `docs/design/spdx-tool/self-test/<unit>.md` — design doc for each SelfTest unit

Per-unit requirements are maintained alongside the design docs:

- `docs/reqstream/spdx-tool/commands/<command>.yaml` — requirements for each command unit
- `docs/reqstream/spdx-tool/self-test/<unit>.yaml` — requirements for each SelfTest unit
