# Workflow Files

While many SpdxTool commands can be executed from the command line, the normal use of the tool is
through YAML workflow files. These files offer the following benefits:

- Comments to explain the purpose behind each step
- Variables to transfer information between steps
- Complex multi-step operations
- Reusable automation scripts

## Basic Structure

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

## Running Workflow Files

To execute a workflow file:

```bash
dotnet spdx-tool run-workflow workflow.yaml
```

To override parameters:

```bash
dotnet spdx-tool run-workflow workflow.yaml parameter1=value1 parameter2=value2
```

## Variables

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

## Environment Variables

Workflow step inputs support token expansion for both workflow variables and environment
variables. Use the `${{ name }}` syntax in any input value:

```yaml
steps:
- command: print
  inputs:
    text:
    - Build version is ${{ BUILD_VERSION }}
```

Token expansion resolves `${{ name }}` against the current workflow variable map first. If
the variable is not found as a workflow variable, it is resolved against the current process
environment. Undefined tokens cause the step to fail with an error.

## Workflow Commands

### Add Package

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

### Update Package

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

### Copy Package

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

### Query

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

### Set Variable

Set a workflow variable:

```yaml
- command: set-variable
  inputs:
    value: Version is ${{ dotnet-version }}
    output: message
```

### Print

Print text to the console:

```yaml
- command: print
  inputs:
    text:
    - Processing SPDX document
    - File: ${{ spdx-file }}
```

### Run Workflow

Run a separate workflow file, URL, or NuGet package:

```yaml
- command: run-workflow
  inputs:
    file: <workflow.yaml>         # Workflow file path (or path within NuGet package)
    url: <url>                    # Optional workflow URL (mutually exclusive with nuget)
    nuget: <package:version>      # Optional NuGet package (mutually exclusive with url)
    integrity: <sha256>           # Optional SHA-256 integrity check for url/nuget workflows
    parameters:
      name: <value>               # Optional workflow parameter
    outputs:
      name: <variable>            # Optional output to save to variable
```

When `nuget` is specified, the `file` path is resolved within the cached NuGet package. The `nuget`
value must be in `PackageName:version` format, for example:

```yaml
- command: run-workflow
  inputs:
    nuget: "DemaConsulting.SpdxWorkflows:1.0.0"
    file: "contentFiles/any/any/workflows/GetDotNetVersion.yaml"
    outputs:
      version: dotnet-version
```

The `nuget` and `url` parameters are mutually exclusive — only one may be specified per step.

## Example Workflow

A complete workflow example that validates an SPDX document and generates a summary:

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
