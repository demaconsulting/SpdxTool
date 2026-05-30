# Introduction

This document defines the verification design for the SpdxTool repository. It covers the
DemaConsulting.SpdxTool command-line system and the DemaConsulting.SpdxTool.Targets MSBuild
integration system, describing how their requirements are verified by automated tests and
self-validation evidence.

## Purpose

The verification design provides review-ready evidence of how repository requirements are proven
by tests. It is intended for developers, reviewers, and compliance stakeholders who need a clear
mapping from software items to the automated verification activities implemented in the test
projects and the built-in self-test suite.

## Scope

This collection covers the checked-in verification design for the DemaConsulting.SpdxTool and
DemaConsulting.SpdxTool.Targets systems, including the Commands, SelfTest, and Utility
subsystems and the listed command and self-test units. OTS verification evidence for all
third-party components consumed by the program is covered in `docs/verification/ots.md` and
`docs/verification/ots/`. It excludes generated outputs under any
generated folder and excludes the internal design of the test projects themselves except where
their test methods provide verification evidence.

## Companion Artifact Structure

- Repository-level introduction and software structure are documented in
  `docs/design/introduction.md`.
- System and unit requirements are maintained in `docs/reqstream/spdx-tool/spdx-tool.yaml`,
  `docs/reqstream/spdx-tool/commands/*.yaml`,
  `docs/reqstream/spdx-tool/self-test/*.yaml`,
  `docs/reqstream/spdx-tool/utility/utility.yaml`,
  `docs/reqstream/spdx-tool/spdx/*.yaml`,
  `docs/reqstream/spdx-tool/context.yaml`,
  `docs/reqstream/spdx-tool/program.yaml`, and
  `docs/reqstream/spdx-tool/platform-requirements.yaml`.
- OTS requirements are maintained in `docs/reqstream/ots/`.
- System and unit design descriptions are maintained in `docs/design/spdx-tool.md`,
  `docs/design/spdx-tool-targets.md`, `docs/design/spdx-tool/`, and
  `docs/design/spdx-tool-targets/`. OTS integration and usage design is in `docs/design/ots.md`
  and `docs/design/ots/`.
- This verification collection is maintained in `docs/verification/`. OTS verification evidence
  is in `docs/verification/ots.md` and `docs/verification/ots/`.
- Implementation source is maintained in `src/DemaConsulting.SpdxTool/` and
  `src/DemaConsulting.SpdxTool.Targets/`.
- Verification evidence is produced by `test/DemaConsulting.SpdxTool.Tests/` and
  `test/DemaConsulting.SpdxTool.Targets.Tests/`.

## References

- [REF-1] SPDX Specification, Version 2.3, SPDX Workgroup.
- [REF-2] The Minimum Elements for a Software Bill of Materials (SBOM), NTIA.
- [REF-3] Microsoft SBOM Tool and Microsoft.Sbom.Targets documentation, Microsoft.
