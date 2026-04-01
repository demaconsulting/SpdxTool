# DemaConsulting.SpdxTool Design Introduction

## Purpose

This document introduces the design of DemaConsulting.SpdxTool, a .NET global tool
for creating, validating, and manipulating SPDX (Software Package Data Exchange)
documents. It serves as the entry point for design documentation supporting formal
code review, compliance evidence, and maintenance activities.

## Scope

This design documentation covers the DemaConsulting.SpdxTool system and all its
constituent subsystems and units. It applies to all source code under `src/` and
test code under `test/`. Third-party (OTS) components are referenced but not
designed in detail.

## Software Structure

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
├── SelfValidation (Subsystem)
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
├── Targets (Subsystem)
├── Spdx (Unit Group)
│   ├── RelationshipDirection.cs (Unit)
│   └── SpdxHelpers.cs (Unit)
├── Utility (Unit Group)
│   ├── PathHelpers.cs (Unit)
│   └── Wildcard.cs (Unit)
├── Context.cs (Unit)
└── Program.cs (Unit)
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
├── SelfValidation/
│   ├── Validate.cs                 — self-validation orchestrator
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

test/DemaConsulting.SpdxTool.Tests/
│   — unit and integration tests for DemaConsulting.SpdxTool
test/DemaConsulting.SpdxTool.Targets.Tests/
    — MSBuild targets integration tests
```
