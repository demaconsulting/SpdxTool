# Introduction

This repository contains two independent software systems: **DemaConsulting.SpdxTool**, a .NET
command-line tool for creating, validating, and manipulating SPDX (Software Package Data Exchange)
documents, and **DemaConsulting.SpdxTool.Targets**, an MSBuild targets extension that integrates
SPDX document decoration into the standard `dotnet pack` workflow. Both systems are local items
with full architectural and detailed design documentation; no OTS items or shared packages have
dedicated design documentation in this repository.

## Purpose

This document defines the design for each software item in SpdxTool — full architectural and
detailed design for local items (systems, subsystems, and units). A reviewer should be able to
understand how each item satisfies its requirements without reading source code. This documentation
supports formal code review, compliance evidence, and maintenance activities.

## Scope

Local items:

- **DemaConsulting.SpdxTool**: system, subsystem, and unit design covering all source under
  `src/DemaConsulting.SpdxTool/`.
- **DemaConsulting.SpdxTool.Targets**: system and unit design covering all source under
  `src/DemaConsulting.SpdxTool.Targets/`.

Out of scope: test projects (`test/`), the build pipeline, and the internal design of any
third-party NuGet packages consumed as dependencies.

## Software Structure

```text
DemaConsulting.SpdxTool (System)
├── Commands (Subsystem)
│   ├── AddPackage.cs (Unit)
│   ├── AddRelationship.cs (Unit)
│   ├── Command.cs (Unit)
│   ├── CommandEntry.cs (Unit)
│   ├── CommandErrorException.cs (Unit)
│   ├── CommandsRegistry.cs (Unit)
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
├── Spdx (Subsystem)
│   ├── RelationshipDirection.cs (Unit)
│   └── SpdxHelpers.cs (Unit)
├── Utility (Subsystem)
│   ├── PathHelpers.cs (Unit)
│   └── Wildcard.cs (Unit)
├── Context.cs (Unit)
└── Program.cs (Unit)

DemaConsulting.SpdxTool.Targets (System)
├── build/DemaConsulting.SpdxTool.Targets.targets (Unit)
└── buildMultiTargeting/DemaConsulting.SpdxTool.Targets.targets (Unit)
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
│   ├── CommandsRegistry.cs          — registry of all available commands
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

## Companion Artifact Structure

Each local software item has corresponding artifacts in parallel directory trees:

- Requirements: `docs/reqstream/{system-name}.yaml`,
  `docs/reqstream/{system-name}[/{subsystem-name}...]/{item}.yaml`
- Design: `docs/design/{system-name}.md`,
  `docs/design/{system-name}[/{subsystem-name}...]/{item}.md`
- Verification: `docs/verification/{system-name}.md`,
  `docs/verification/{system-name}[/{subsystem-name}...]/{item}.md`
- Source: `src/{SystemName}[/{SubsystemName}...]/{Item}.cs`
- Tests: `test/{SystemName}.Tests[/{SubsystemName}...]/{Item}Tests.cs`

OTS items: N/A — no OTS items have dedicated design documentation in this repository.

Shared packages: N/A — no shared packages are consumed from other repositories in this program.

Review-sets: defined in `.reviewmark.yaml`.

## References

- [SpdxTool releases](https://github.com/demaconsulting/SpdxTool/releases)
- [SPDX Specification](https://spdx.github.io/spdx-spec/v2.3/)
