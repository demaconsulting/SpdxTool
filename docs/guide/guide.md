# SpdxTool User Guide

## Introduction

SpdxTool is a .NET tool for manipulating SPDX (Software Package Data Exchange) SBOM (Software Bill of Materials)
files. This guide provides comprehensive documentation for installing, configuring, and using SpdxTool in your
software development and compliance workflows.

### Purpose

SpdxTool simplifies the process of working with SPDX SBOM files by:

* Providing command-line utilities for SPDX document manipulation
* Enabling workflow automation through YAML configuration files
* Supporting comprehensive SBOM operations (create, read, update, validate)
* Facilitating compliance and security analysis
* Enabling integration with CI/CD pipelines

### Key Features

* **Command-Line Interface**: Rich set of commands for SPDX operations
* **Workflow Automation**: YAML-based workflow files for complex operations
* **SPDX Validation**: Validate SPDX documents for correctness and NTIA compliance
* **Package Management**: Add, update, copy, and query packages
* **Relationship Management**: Manage relationships between SPDX elements
* **Markdown Export**: Generate human-readable markdown from SPDX documents
* **Mermaid Diagrams**: Visualize SPDX relationships as diagrams
* **Self-Validation**: Built-in validation for tool qualification

## Installation

### Prerequisites

Before installing SpdxTool, ensure you have:

* **.NET SDK**: Version 8.0 or later
* **Operating System**: Windows, Linux, or macOS

### Installation Methods

#### Local Installation

To add SpdxTool to a .NET tool manifest file:

```bash
dotnet new tool-manifest # if you are setting up this repo
dotnet tool install --local DemaConsulting.SpdxTool
```

The tool can then be executed by:

```bash
dotnet spdx-tool <arguments>
```

#### Global Installation

For global installation across all projects:

```bash
dotnet tool install --global DemaConsulting.SpdxTool
```

Then execute directly:

```bash
spdx-tool <arguments>
```

### Verifying Installation

To verify SpdxTool is installed correctly:

```bash
dotnet spdx-tool --version
```

This will display the installed version number.

## Command-Line Usage

### General Syntax

The general command-line syntax is:

```bash
spdx-tool [options] <command> [arguments]
```

### Global Options

* `-h, --help` - Show help message and exit
* `-v, --version` - Show version information and exit
* `-l, --log <log-file>` - Log output to file
* `-s, --silent` - Silence console output
* `--validate` - Perform self-validation
* `-r, --result <file>` - Self-validation result TRX file

### Available Commands

* `help <command>` - Display extended help about a command
* `add-package` - Add package to SPDX document (workflow only)
* `add-relationship <spdx.json> <args>` - Add relationship between elements
* `copy-package <spdx.json> <args>` - Copy package between SPDX documents (workflow only)
* `diagram <spdx.json> <mermaid.txt>` - Generate mermaid diagram
* `find-package <spdx.json> <criteria>` - Find package ID in SPDX document
* `get-version <spdx.json> <criteria>` - Get the version of an SPDX package
* `hash <operation> <algorithm> <file>` - Generate or verify hashes of files
* `print <text>` - Print text to the console
* `query <pattern> <program> [args]` - Query program output for value
* `rename-id <arguments>` - Rename an element ID in an SPDX document
* `run-workflow <workflow.yaml>` - Runs the workflow file/url
* `set-variable` - Set workflow variable (workflow only)
* `to-markdown <spdx.json> <out.md> [args]` - Create Markdown summary for SPDX document
* `update-package` - Update package in SPDX document (workflow only)
* `validate <spdx.json> [ntia]` - Validate SPDX document for issues

### Getting Command Help

To get detailed help for any command:

```bash
dotnet spdx-tool help <command>
```

For example:

```bash
dotnet spdx-tool help validate
```

## Core Commands

### Validate Command

The `validate` command checks an SPDX document for correctness and optionally for NTIA compliance.

**Syntax:**

```bash
spdx-tool validate <spdx.json> [ntia]
```

**Example:**

```bash
dotnet spdx-tool validate manifest.spdx.json
dotnet spdx-tool validate manifest.spdx.json ntia
```

### Add Relationship Command

The `add-relationship` command adds relationships between SPDX elements.

**Syntax:**

```bash
spdx-tool add-relationship <spdx.json> <id> <type> <element> [comment]
```

**Example:**

```bash
dotnet spdx-tool add-relationship manifest.spdx.json SPDXRef-Package DEPENDS_ON SPDXRef-Library
```

### Find Package Command

The `find-package` command locates a package in an SPDX document based on criteria.

**Syntax:**

```bash
spdx-tool find-package <spdx.json> [criteria]
```

**Example:**

```bash
dotnet spdx-tool find-package manifest.spdx.json name=MyPackage
dotnet spdx-tool find-package manifest.spdx.json version=1.0.0
```

### Get Version Command

The `get-version` command retrieves the version of a package in an SPDX document.

**Syntax:**

```bash
spdx-tool get-version <spdx.json> [criteria]
```

**Example:**

```bash
dotnet spdx-tool get-version manifest.spdx.json id=SPDXRef-Package
```

### To Markdown Command

The `to-markdown` command generates a human-readable markdown summary of an SPDX document.

**Syntax:**

```bash
spdx-tool to-markdown <spdx.json> <out.md> [title] [depth]
```

**Example:**

```bash
dotnet spdx-tool to-markdown manifest.spdx.json summary.md "SBOM Summary"
```

### Diagram Command

The `diagram` command generates a mermaid diagram visualizing SPDX relationships.

**Syntax:**

```bash
spdx-tool diagram <spdx.json> <mermaid.txt>
```

**Example:**

```bash
dotnet spdx-tool diagram manifest.spdx.json diagram.mmd
```

### Hash Command

The `hash` command generates or verifies file hashes.

**Syntax:**

```bash
spdx-tool hash <operation> <algorithm> <file>
```

**Example:**

```bash
dotnet spdx-tool hash generate sha256 myfile.txt
dotnet spdx-tool hash verify sha256 myfile.txt
```

## Workflow Files

While many SpdxTool commands can be executed from the command line, the normal use of the tool is through YAML
workflow files. These files have the benefit of:

* Comments to explain the purpose behind each step
* Variables to transfer information between steps
* Complex multi-step operations
* Reusable automation scripts

### Basic Structure

SpdxTool workflow files have the following basic structure:

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

### Running Workflow Files

To execute a workflow file:

```bash
dotnet spdx-tool run-workflow workflow.yaml
```

To override parameters:

```bash
dotnet spdx-tool run-workflow workflow.yaml parameter1=value1 parameter2=value2
```

### Variables

Variables can be declared at the top of the workflow file:

```yaml
parameters:
  spdx-file: manifest.spdx.json
  output-file: summary.md
```

Variables can be expanded in step inputs using dollar expansion syntax:

```yaml
steps:
- command: to-markdown
  inputs:
    spdx: ${{ spdx-file }}
    markdown: ${{ output-file }}
```

Variables can be set or modified during workflow execution:

```yaml
steps:
- command: get-version
  inputs:
    spdx: manifest.spdx.json
    id: SPDXRef-Package
    output: package-version

- command: print
  inputs:
    text:
    - Package version is ${{ package-version }}
```

### Workflow Commands

#### Add Package

Add a new package to an SPDX document:

```yaml
- command: add-package
  inputs:
    spdx: manifest.spdx.json
    package:
      id: SPDXRef-NewPackage
      name: MyPackage
      download: https://example.com/package.tar.gz
      version: 1.0.0
      license: MIT
    relationships:
    - type: DEPENDS_ON
      element: SPDXRef-Document
```

#### Update Package

Update an existing package in an SPDX document:

```yaml
- command: update-package
  inputs:
    spdx: manifest.spdx.json
    package:
      id: SPDXRef-Package
      version: 2.0.0
      download: https://example.com/package-v2.tar.gz
```

#### Copy Package

Copy a package from one SPDX document to another:

```yaml
- command: copy-package
  inputs:
    from: source.spdx.json
    to: target.spdx.json
    package: SPDXRef-Package
    recursive: true
    files: true
```

#### Query Command

Query information from program output:

```yaml
- command: query
  inputs:
    output: dotnet-version
    pattern: '(?<value>\d+\.\d+\.\d+)'
    program: dotnet
    arguments:
    - '--version'
```

#### Set Variable

Set a workflow variable:

```yaml
- command: set-variable
  inputs:
    value: Version is ${{ dotnet-version }}
    output: message
```

#### Print Command

Print text to the console:

```yaml
- command: print
  inputs:
    text:
    - Processing SPDX document
    - File: ${{ spdx-file }}
```

### Example Workflow

Here's a complete workflow example that validates an SPDX document and generates a summary:

```yaml
# Workflow parameters
parameters:
  spdx-file: manifest.spdx.json
  output-file: summary.md

# Workflow steps
steps:
# Validate the SPDX document
- command: validate
  inputs:
    spdx: ${{ spdx-file }}
    ntia: true

# Print validation success
- command: print
  inputs:
    text:
    - SPDX document is valid

# Generate markdown summary
- command: to-markdown
  inputs:
    spdx: ${{ spdx-file }}
    markdown: ${{ output-file }}
    title: Software Bill of Materials

# Print completion message
- command: print
  inputs:
    text:
    - Summary generated at ${{ output-file }}
```

## Self-Validation

### Validation Purpose

Self-validation produces a report demonstrating that SpdxTool is functioning correctly. This is useful in regulated
industries where tool validation evidence is required.

### Running Validation

To perform self-validation:

```bash
dotnet spdx-tool --validate
```

To save the validation report to a file:

```bash
dotnet spdx-tool --log validation.log --validate
```

To generate a TRX test results file:

```bash
dotnet spdx-tool --validate --result validation.trx
```

### Validation Report

The validation report contains:

* SpdxTool version
* Machine name
* Operating system version
* .NET runtime version
* Timestamp
* Test results

Example validation report:

```text
# DemaConsulting.SpdxTool

| Information         | Value                                              |
| :------------------ | :------------------------------------------------- |
| SpdxTool Version    | 2.6.0                                              |
| Machine Name        | BUILD-SERVER                                       |
| OS Version          | Microsoft Windows NT 10.0.19045.0                  |
| DotNet Runtime      | .NET 8.0.0                                         |
| Time Stamp          | 2024-01-15T10:30:00Z                               |

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

### Validation Failure

On validation failure:

* The tool exits with a non-zero exit code
* The report indicates which validation tests failed
* Error messages provide diagnostic information

## CI/CD Integration

### GitHub Actions

Example GitHub Actions workflow:

```yaml
name: SBOM Validation

on: [push, pull_request]

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.x'
      
      - name: Install SpdxTool
        run: dotnet tool install --global DemaConsulting.SpdxTool
      
      - name: Generate SBOM
        run: |
          # Your SBOM generation commands here
          
      - name: Validate SBOM
        run: spdx-tool validate manifest.spdx.json ntia
      
      - name: Generate Summary
        run: spdx-tool to-markdown manifest.spdx.json sbom-summary.md
      
      - name: Upload SBOM
        uses: actions/upload-artifact@v4
        with:
          name: sbom
          path: |
            manifest.spdx.json
            sbom-summary.md
```

### Azure DevOps

Example Azure DevOps pipeline:

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: UseDotNet@2
    inputs:
      version: '8.x'
  
  - script: |
      dotnet tool install --global DemaConsulting.SpdxTool
    displayName: 'Install SpdxTool'
  
  - script: |
      spdx-tool validate manifest.spdx.json ntia
    displayName: 'Validate SBOM'
  
  - script: |
      spdx-tool to-markdown manifest.spdx.json sbom-summary.md
    displayName: 'Generate SBOM Summary'
  
  - task: PublishBuildArtifacts@1
    inputs:
      pathToPublish: 'manifest.spdx.json'
      artifactName: 'sbom'
```

## Use Cases

### SBOM Generation and Validation

A common workflow for generating and validating SBOMs:

```yaml
parameters:
  component-path: src/MyApp
  output-spdx: manifest.spdx.json

steps:
# Generate SBOM using Microsoft SBOM Tool
- command: print
  inputs:
    text:
    - Generating SBOM for ${{ component-path }}

# Validate the generated SBOM
- command: validate
  inputs:
    spdx: ${{ output-spdx }}
    ntia: true

# Generate markdown summary
- command: to-markdown
  inputs:
    spdx: ${{ output-spdx }}
    markdown: sbom-summary.md
    title: Software Bill of Materials
```

### Dependency Version Tracking

Track and update dependency versions:

```yaml
parameters:
  spdx-file: manifest.spdx.json

steps:
# Get current .NET SDK version
- command: query
  inputs:
    output: dotnet-version
    pattern: '(?<value>\d+\.\d+\.\d+)'
    program: dotnet
    arguments:
    - '--version'

# Find .NET SDK package in SPDX
- command: find-package
  inputs:
    output: dotnet-package-id
    spdx: ${{ spdx-file }}
    name: .NET SDK

# Update .NET SDK version
- command: update-package
  inputs:
    spdx: ${{ spdx-file }}
    package:
      id: ${{ dotnet-package-id }}
      version: ${{ dotnet-version }}
```

### Multi-Document SBOM Assembly

Combine multiple SPDX documents:

```yaml
parameters:
  base-spdx: base.spdx.json
  component-spdx: component.spdx.json
  output-spdx: combined.spdx.json

steps:
# Copy base document to output
- command: print
  inputs:
    text:
    - Combining SPDX documents

# Copy component packages to output
- command: copy-package
  inputs:
    from: ${{ component-spdx }}
    to: ${{ output-spdx }}
    package: SPDXRef-Component
    recursive: true
    files: true
    relationships:
    - type: DEPENDS_ON
      element: SPDXRef-Document

# Validate combined document
- command: validate
  inputs:
    spdx: ${{ output-spdx }}
```

## Best Practices

### SPDX Document Organization

* **Consistent Naming**: Use consistent ID naming conventions (e.g., SPDXRef-Component-Name)
* **Document Structure**: Organize packages hierarchically with clear relationships
* **Version Control**: Keep SPDX documents in version control
* **Automation**: Use workflow files for repeatable SBOM operations

### Workflow Design

* **Modularity**: Break complex operations into reusable workflow files
* **Variables**: Use variables for configurable values
* **Comments**: Add comments to explain workflow logic
* **Validation**: Always validate SPDX documents after modifications
* **Error Handling**: Check command outputs and handle failures

### CI/CD Best Practices

* **Automated Generation**: Generate SBOMs automatically in build pipelines
* **Validation Gates**: Fail builds on SBOM validation errors
* **Artifact Publishing**: Publish SBOMs as build artifacts
* **NTIA Compliance**: Validate for NTIA minimum elements when required
* **Documentation**: Generate markdown summaries for human review

### Security and Compliance

* **Regular Updates**: Keep SpdxTool updated to the latest version
* **License Compliance**: Ensure all package licenses are correctly specified
* **Vulnerability Tracking**: Integrate with vulnerability databases
* **Audit Trail**: Log all SBOM operations for audit purposes
* **Access Control**: Restrict SBOM modifications to authorized processes

## Troubleshooting

### Common Issues

#### Invalid SPDX Document

**Problem**: Validation fails with schema errors.

**Solution**:

* Ensure the SPDX document conforms to the SPDX 2.3 specification
* Check that all required fields are present
* Verify JSON syntax is correct
* Use the validate command to get detailed error messages

#### Package Not Found

**Problem**: find-package or get-version commands cannot locate a package.

**Solution**:

* Verify the package exists in the SPDX document
* Check search criteria match exactly (case-sensitive)
* Use multiple criteria to narrow the search
* List all packages with to-markdown to see available packages

#### Workflow Variable Not Expanding

**Problem**: Variables in workflow files are not being replaced.

**Solution**:

* Ensure variable syntax is correct: `${{ variable-name }}`
* Check that the variable is defined in parameters or set by a previous step
* Verify the output parameter name matches the variable name
* Variables are case-sensitive

#### Permission Errors

**Problem**: Cannot write SPDX document or output files.

**Solution**:

* Ensure the output directory exists
* Verify write permissions on the output directory
* Check disk space availability
* Use an absolute path or verify the working directory

### Debug Mode

Enable detailed logging for troubleshooting:

```bash
dotnet spdx-tool --log debug.log <command> <arguments>
```

This provides detailed information about:

* Command execution
* File operations
* Variable expansion
* Error stack traces

## Appendix

### SPDX Specification

SpdxTool supports the SPDX 2.3 specification. For details, see:

* [SPDX Specification](https://spdx.github.io/spdx-spec/)
* [SPDX GitHub](https://github.com/spdx/spdx-spec)

### NTIA Minimum Elements

The NTIA minimum elements for SBOM include:

* Author name
* Timestamp
* Component name
* Version string
* Component identifiers (PURL, CPE)
* Dependency relationships
* Author of SBOM data

For more information, see: [NTIA SBOM Minimum Elements](https://www.ntia.gov/files/ntia/publications/sbom_minimum_elements_report.pdf)

### Version History

See the [GitHub releases page](https://github.com/demaconsulting/SpdxTool/releases) for detailed version history.

### License

SpdxTool is licensed under the MIT License. See the
[LICENSE](https://github.com/demaconsulting/SpdxTool/blob/main/LICENSE) file for details.

### Contributing

Contributions are welcome! Please see the
[Contributing Guidelines](https://github.com/demaconsulting/SpdxTool/blob/main/CONTRIBUTING.md) for details.

### Support

For issues, questions, or feature requests:

* **GitHub Issues**: <https://github.com/demaconsulting/SpdxTool/issues>
* **Documentation**: <https://github.com/demaconsulting/SpdxTool>

### Additional Resources

* **SPDX Official Site**: <https://spdx.dev/>
* **Microsoft SBOM Tool**: <https://github.com/microsoft/sbom-tool>
* **.NET Tool Documentation**: <https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools>
* **SPDX Model Library**: <https://github.com/demaconsulting/SpdxModel>
