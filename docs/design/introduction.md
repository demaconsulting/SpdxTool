# Introduction

This repository contains two independent software systems: **DemaConsulting.SpdxTool**, a .NET
command-line tool for creating, validating, and manipulating SPDX (Software Package Data Exchange)
documents, and **DemaConsulting.SpdxTool.Targets**, an MSBuild targets extension that integrates
SPDX document decoration into the standard `dotnet pack` workflow. Both systems are local items
with full architectural and detailed design documentation. OTS integration and usage designs are
also included in this document for all third-party components consumed by the program.

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

OTS items: integration and usage design for all third-party components consumed by the program;
see `docs/design/ots.md` and `docs/design/ots/`.

Out of scope: test projects (`test/`), the internal design of third-party NuGet packages, and
the internal design of CI pipeline tools.

## Software Structure

The software structure is modeled in SysML2 under `docs/sysml2/` and rendered to the
diagram below by SysML2Tools as part of the build pipeline. AI agents should query the
SysML2 model directly (see the `sysml2tools-query` skill) rather than parsing this
diagram or the prose below. The model contains both systems: **SpdxTool**
(DemaConsulting.SpdxTool) with its Commands, SelfTest, Spdx, and Utility subsystems and its
Program and Context units, and **SpdxToolTargets** (DemaConsulting.SpdxTool.Targets) with its
TargetsSingle (`build/`) and TargetsMulti (`buildMultiTargeting/`) units. The runtime OTS
packages (DemaConsulting.SpdxModel, DemaConsulting.NuGet.Caching, DemaConsulting.TestResults,
and YamlDotNet) are modeled as parts of SpdxTool; the test-framework and CI pipeline OTS items
are documented in `docs/design/ots.md` rather than modeled as parts.

![Software Structure](SoftwareStructureView.svg)

The diagram below complements the structure view by showing how data flows at run time:
Program parses the command line into a Context and dispatches to a command (or, with
`--validate`, to the SelfTest subsystem); the Commands read SPDX documents and workflow files
through the Spdx and Utility helpers and write the updated SPDX documents and reports; the
SpdxTool.Targets MSBuild targets invoke the tool during `dotnet pack`.

![SpdxTool architecture: the spdx-tool Program entry point builds a Context and dispatches to
the Commands, which use the Spdx and Utility helpers to turn SPDX files and workflow files into
updated SPDX files, Markdown and Mermaid reports, and SHA-256 hash files, while SelfTest runs the
built-in checks and the SpdxTool.Targets MSBuild targets run a workflow during dotnet
pack](architecture.svg)

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
│   ├── TemporaryDirectory.cs       — disposable temporary workspace helper
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

Note: `docs/reqstream/spdx-tool/platform-requirements.yaml` is an auxiliary requirements file for
platform-specific decomposition. It sits outside the standard subsystem naming convention and does
not follow the `{subsystem-name}.yaml` pattern; renaming it would break the ReqStream section-merge
linkage with the parent `spdx-tool.yaml`.

- Design: `docs/design/{system-name}.md`,
  `docs/design/{system-name}[/{subsystem-name}...]/{item}.md`
- Verification: `docs/verification/{system-name}.md`,
  `docs/verification/{system-name}[/{subsystem-name}...]/{item}.md`
- Source: `src/{SystemName}[/{SubsystemName}...]/{Item}.cs`
- Tests: `test/{SystemName}.Tests[/{SubsystemName}...]/{Item}Tests.cs`

OTS items: `docs/design/ots.md` and `docs/design/ots/{ots-name}.md` — integration and usage
design; `docs/verification/ots.md` and `docs/verification/ots/{ots-name}.md` — verification
evidence; `docs/reqstream/ots/{ots-name}.yaml` — requirements.

Shared packages: N/A — no shared packages are consumed from other repositories in this program.

Review-sets: defined in `.reviewmark.yaml`.

## References

- [REF-1] SPDX Specification, Version 2.3, SPDX Workgroup. <https://spdx.github.io/spdx-spec/v2.3/>
