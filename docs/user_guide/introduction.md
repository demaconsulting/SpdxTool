# Introduction

This guide describes how to install, configure, and use SpdxTool.

## Purpose

SpdxTool is a .NET tool for creating, validating, and manipulating SPDX (Software Package Data Exchange)
SBOM (Software Bill of Materials) files. It provides a command-line interface for SBOM document
manipulation, YAML-based workflow automation for complex multi-step operations, and MSBuild integration
for automatic SBOM decoration during package builds.

SpdxTool supports a wide range of SBOM operations including package management, relationship management,
validation, markdown export, and Mermaid diagram generation. A built-in self-validation system produces
evidence of correct tool operation, which is useful in regulated industries where tool qualification
evidence is required.

The workflow below shows how SpdxTool is used: start from an SPDX SBOM and a workflow file, run the
`spdx-tool` command locally, in a CI pipeline, or through the SpdxTool.Targets package during `dotnet pack`,
and use the updated SBOM, Markdown and Mermaid reports, and exit code as decorated packages, a CI quality
gate, release documentation, and supply-chain compliance evidence.

![SpdxTool workflow: start from an SPDX 2.x JSON SBOM, other SPDX documents, and workflow YAML files
(local or from a URL or NuGet package), run spdx-tool commands, run-workflow, or validate, or let the
SpdxTool.Targets package run a workflow during dotnet pack, get an updated SPDX document, a Markdown
summary, a Mermaid diagram, and a pass/fail exit code, then use them for decorated packages, a CI
quality gate, release documentation, and supply-chain compliance](workflow.svg)

## Scope

This guide covers installation, configuration, and use of SpdxTool on Windows, Linux, and macOS.
Prerequisites: .NET SDK 10.0 or 11.0. The guide includes:

- Installation instructions (local and global)
- Command-line reference and core command descriptions
- YAML workflow file authoring and execution
- MSBuild integration via DemaConsulting.SpdxTool.Targets
- Self-validation for tool qualification
- CI/CD integration examples
- Common use cases, best practices, and troubleshooting guidance

## References

- [REF-1] SPDX Specification, Version 2.3, SPDX Workgroup. <https://spdx.github.io/spdx-spec/v2.3/>
- [REF-2] NTIA Minimum Elements for a Software Bill of Materials, NTIA. <https://www.ntia.gov/files/ntia/publications/sbom_minimum_elements_report.pdf>
- [REF-3] SpdxTool releases. <https://github.com/demaconsulting/SpdxTool/releases>
