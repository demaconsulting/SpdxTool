# SpdxTool

<!-- IMPORTANT: All links in this file must be absolute URLs.
     This file is distributed in packages and relative links will not resolve. -->

![GitHub forks](https://img.shields.io/github/forks/demaconsulting/SpdxTool?style=plastic)
![GitHub Repo stars](https://img.shields.io/github/stars/demaconsulting/SpdxTool?style=plastic)
![GitHub contributors](https://img.shields.io/github/contributors/demaconsulting/SpdxTool?style=plastic)
![GitHub](https://img.shields.io/github/license/demaconsulting/SpdxTool?style=plastic)
![Build](https://github.com/demaconsulting/SpdxTool/actions/workflows/build_on_push.yaml/badge.svg)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=demaconsulting_SpdxTool&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=demaconsulting_SpdxTool)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=demaconsulting_SpdxTool&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=demaconsulting_SpdxTool)
[![NuGet Version](https://img.shields.io/nuget/v/DemaConsulting.SpdxTool?style=plastic)](https://www.nuget.org/packages/DemaConsulting.SpdxTool)

.NET tool for manipulating SPDX SBOM files

## Overview

DemaConsulting.SpdxTool is a .NET tool for creating, validating, and manipulating SPDX
(Software Package Data Exchange) documents. The repository also contains
DemaConsulting.SpdxTool.Targets, an MSBuild targets extension that integrates SPDX document
decoration into the standard `dotnet pack` build workflow.

## Features

- Create, validate, and manipulate SPDX (Software Package Data Exchange) documents from the command line.
- Drive SBOM operations through workflow YAML files for repeatable, automated pipelines.
- Self-validation system generates evidence of tool correctness for regulated environments.
- MSBuild targets integration automatically decorates SBOMs during `dotnet pack`.
- Multi-command CLI supporting add-package, validate, copy-package, to-markdown, diagram, and more.
- Multi-framework support targeting .NET 8, .NET 9, and .NET 10.
- Continuous compliance evidence generated automatically on every CI run.

## Installation

The following will add SpdxTool to a .NET tool manifest file:

```bash
dotnet new tool-manifest # if you are setting up this repo
dotnet tool install --local DemaConsulting.SpdxTool
```

The tool can then be executed by:

```bash
dotnet spdx-tool <arguments>
```

## Usage

Validate an SPDX document:

```bash
dotnet spdx-tool validate sbom.spdx.json
```

Run a workflow file:

```bash
dotnet spdx-tool run-workflow spdx-workflow.yaml
```

Full command reference:

```text
Usage: spdx-tool [options] <command> [arguments]

Options:
  -h, -?, --help                           Show this help message and exit
  -v, --version                            Show version information and exit
  -l, --log <log-file>                     Log output to file
  -s, --silent                             Silence console output
      --validate                           Perform self-validation
  -r, --result <file>                      Self-validation result file (.trx TRX or .xml JUnit XML)
      --depth <level>                      Self-validation report depth level

Commands:
  help <command>                           Display extended help about a command
  add-package                              Add package to SPDX document (workflow only).
  add-relationship <spdx.json> <args>      Add relationship between elements.
  copy-package <spdx.json> <args>          Copy package between SPDX documents.
  diagram <spdx.json> <mermaid.txt> [tools] Generate mermaid diagram.
  find-package <spdx.json> <criteria>      Find package ID in SPDX document
  get-version <spdx.json> <criteria>       Get the version of an SPDX package.
  hash <operation> <algorithm> <file>      Generate or verify hashes of files
  print <text>                             Print text to the console
  query <pattern> <program> [args]         Query program output for value
  rename-id <arguments>                    Rename an element ID in an SPDX document.
  run-workflow <workflow.yaml>             Runs the workflow file/url
  set-variable                             Set workflow variable (workflow only).
  to-markdown <spdx.json> <out.md> [args]  Create Markdown summary for SPDX document
  update-package                           Update package in SPDX document (workflow only).
  validate <spdx.json> [ntia]              Validate SPDX document for issues
```

## Building

```pwsh
pwsh ./build.ps1
```

## User Guide

The SpdxTool User Guide is available on the
[SpdxTool releases page](https://github.com/demaconsulting/SpdxTool/releases).

## Contributing

See [CONTRIBUTING.md](https://github.com/demaconsulting/SpdxTool/blob/main/CONTRIBUTING.md) for
guidelines on setting up your development environment, coding standards, running tests, and
submitting pull requests.

Before contributing, please read our
[Code of Conduct](https://github.com/demaconsulting/SpdxTool/blob/main/CODE_OF_CONDUCT.md).

## License

This project is licensed under the MIT License — see
[LICENSE](https://github.com/demaconsulting/SpdxTool/blob/main/LICENSE) for details.

By contributing to this project, you agree that your contributions will be licensed under the
MIT License.

## Support

- [Report a bug or request a feature](https://github.com/demaconsulting/SpdxTool/issues)
- [Ask a question or start a discussion](https://github.com/demaconsulting/SpdxTool/discussions)

## Workflow YAML Files

The SpdxTool can be driven using workflow YAML files of the following format:

```yaml
# Workflow parameters
parameters:
  parameter-name: value

# Workflow steps
steps:
  - command: <command-name>
    inputs:
      <arguments mapping>

  - command: <command-name>
    inputs:
      input1: value
      input2: ${{ parameter-name }}
```

## Self Validation

Running self-validation produces a report containing the following information:

```text
# DemaConsulting.SpdxTool

| Information         | Value                                              |
| :------------------ | :------------------------------------------------- |
| SpdxTool Version    | <version>                                          |
| Machine Name        | <machine-name>                                     |
| OS Version          | <os-version>                                       |
| DotNet Runtime      | <dotnet-runtime-version>                           |
| Time Stamp          | <timestamp>                                        |

✓ SpdxTool_AddPackage - Passed
✓ SpdxTool_AddRelationship - Passed
✓ SpdxTool_Basic - Passed
✓ SpdxTool_CopyPackage - Passed
✓ SpdxTool_Diagram - Passed
✓ SpdxTool_FindPackage - Passed
✓ SpdxTool_GetVersion - Passed
✓ SpdxTool_Hash - Passed
✓ SpdxTool_Ntia - Passed
✓ SpdxTool_Query - Passed
✓ SpdxTool_RenameId - Passed
✓ SpdxTool_RunNuGetWorkflow - Passed
✓ SpdxTool_ToMarkdown - Passed
✓ SpdxTool_UpdatePackage - Passed

Total Tests: 14
Passed: 14
Failed: 0

Validation Passed
```

Each test in the report proves a specific command works correctly:

- **SpdxTool_AddPackage** - `add-package` command adds a package with relationships to an SPDX file.
- **SpdxTool_AddRelationship** - `add-relationship` command adds a relationship between SPDX elements.
- **SpdxTool_Basic** - `validate` command accepts valid and rejects invalid SPDX files.
- **SpdxTool_CopyPackage** - `copy-package` command copies a package with relationships between SPDX files.
- **SpdxTool_Diagram** - `diagram` command generates a Mermaid diagram from an SPDX file.
- **SpdxTool_FindPackage** - `find-package` command locates a package by name in an SPDX file.
- **SpdxTool_GetVersion** - `get-version` command retrieves a package version from an SPDX file.
- **SpdxTool_Hash** - `hash` command generates and verifies file hashes.
- **SpdxTool_Ntia** - `validate` command enforces NTIA minimum SBOM element requirements.
- **SpdxTool_Query** - `query` command extracts values from program output.
- **SpdxTool_RenameId** - `rename-id` command renames an element identifier throughout an SPDX file.
- **SpdxTool_RunNuGetWorkflow** - `run-workflow` command executes a workflow from a NuGet package.
- **SpdxTool_ToMarkdown** - `to-markdown` command generates a Markdown summary from an SPDX file.
- **SpdxTool_UpdatePackage** - `update-package` command updates all fields of a package in an SPDX file.

On validation failure the tool will exit with a non-zero exit code.

This report may be useful in regulated industries requiring evidence of tool validation.

## Project Quality

This project maintains high code quality standards:

- ✓ Comprehensive unit test coverage
- ✓ Static code analysis with multiple analyzers
- ✓ Continuous integration with SonarCloud
- ✓ Self-validation system for tool correctness
- ✓ Warnings treated as errors
- ✓ EditorConfig for consistent code style
- ✓ **Continuous Compliance**: Compliance evidence generated automatically on every CI run,
  following the [Continuous Compliance][link-continuous-compliance] methodology

## Additional Information

Additional information can be found at:

- [Architecture Documentation](https://github.com/demaconsulting/SpdxTool/blob/main/ARCHITECTURE.md)
- [SPDX Specification](https://spdx.dev/)

[link-continuous-compliance]: https://demaconsulting.github.io/SpdxTool/articles/continuous-compliance.html
