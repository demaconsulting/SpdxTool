# SPDX Tool

![GitHub forks](https://img.shields.io/github/forks/demaconsulting/SpdxTool?style=plastic)
![GitHub Repo stars](https://img.shields.io/github/stars/demaconsulting/SpdxTool?style=plastic)
![GitHub contributors](https://img.shields.io/github/contributors/demaconsulting/SpdxTool?style=plastic)
![GitHub](https://img.shields.io/github/license/demaconsulting/SpdxTool?style=plastic)
![Build](https://github.com/demaconsulting/SpdxTool/actions/workflows/build_on_push.yaml/badge.svg)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=demaconsulting_SpdxTool&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=demaconsulting_SpdxTool)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=demaconsulting_SpdxTool&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=demaconsulting_SpdxTool)
[![NuGet Version](https://img.shields.io/nuget/v/DemaConsulting.SpdxTool?style=plastic)](https://www.nuget.org/packages/DemaConsulting.SpdxTool)

Dotnet tool for manipulating SPDX SBOM files

## Installation

The following will add SpdxTool to a Dotnet tool manifest file:

```bash
dotnet new tool-manifest # if you are setting up this repo
dotnet tool install --local DemaConsulting.SpdxTool
```

The tool can then be executed by:

```bash
dotnet spdx-tool <arguments>
```

## Usage

The following shows the command-line usage of SpdxTool:

```text
Usage: spdx-tool [options] <command> [arguments]

Options:
  -h, --help                               Show this help message and exit
  -v, --version                            Show version information and exit
  -l, --log <log-file>                     Log output to file
  -s, --silent                             Silence console output
      --validate                           Perform self-validation
  -r, --result <file>                      Self-validation result TRX file

Commands:
  help <command>                           Display extended help about a command
  add-package                              Add package to SPDX document (workflow only).
  add-relationship <spdx.json> <args>      Add relationship between elements.
  copy-package <spdx.json> <args>          Copy package between SPDX documents (workflow only).
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

A more detailed description of the usage can be found in the [command-line documentation][command-line-docs]

## Workflow YAML Files

The SpdxTool can be driven using workflow yaml files of the following format:

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

A more detailed description of workflow YAML files can be found in the [workflow documentation][workflow-docs]

## Self Validation

Running self-validation produces a report containing the following information:

```text
# DemaConsulting.SpdxTool

| Information         | Value                                              |
| :------------------ | :------------------------------------------------- |
| SpdxTool Version    | <version>                                         |
| Machine Name        | <machine-name>                                     |
| OS Version          | <os-version>                                       |
| DotNet Runtime      | <dotnet-runtime-version>                           |
| Time Stamp          | <timestamp>                                        |

Tests:

- AddPackage: Passed
- AddRelationship: Passed
- CopyPackage: Passed
- FindPackage: Passed
- GetVersion: Passed
- Query: Passed
- RenameId: Passed
- UpdatePackage: Passed

Validation Passed
```

On validation failure the tool will exit with a non-zero exit code.

This report may be useful in regulated industries requiring evidence of tool validation.

## Contributing

We welcome contributions! Please see our [Contributing Guide][contributing] for details on:

- Setting up your development environment
- Coding standards and conventions
- Running tests and quality checks
- Submitting pull requests

Before contributing, please read our [Code of Conduct][code-of-conduct].

## Project Quality

This project maintains high code quality standards:

- ✓ Comprehensive unit test coverage
- ✓ Static code analysis with multiple analyzers
- ✓ Continuous integration with SonarCloud
- ✓ Self-validation system for tool correctness
- ✓ Warnings treated as errors
- ✓ EditorConfig for consistent code style

## Additional Information

Additional information can be found at:

- [Architecture Documentation][architecture]
- [SPDX Site][spdx-site]
- [GitHub CI][github-ci-docs]
- [Using with Microsoft SBOM Tool][sbom-tool-docs]

[command-line-docs]: https://github.com/demaconsulting/SpdxTool/blob/main/docs/spdx-tool-command-line.md
[workflow-docs]: https://github.com/demaconsulting/SpdxTool/blob/main/docs/spdx-tool-workflow-files.md
[contributing]: https://github.com/demaconsulting/SpdxTool/blob/main/CONTRIBUTING.md
[code-of-conduct]: https://github.com/demaconsulting/SpdxTool/blob/main/CODE_OF_CONDUCT.md
[architecture]: https://github.com/demaconsulting/SpdxTool/blob/main/ARCHITECTURE.md
[spdx-site]: https://spdx.dev/
[github-ci-docs]: https://github.com/demaconsulting/SpdxTool/blob/main/docs/spdx-tool-github-ci.md
[sbom-tool-docs]: https://github.com/demaconsulting/SpdxTool/blob/main/docs/spdx-tool-and-sbom-tool.md
