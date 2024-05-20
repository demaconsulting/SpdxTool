# SPDX Tool

![GitHub forks](https://img.shields.io/github/forks/demaconsulting/SpdxTool?style=plastic)
![GitHub Repo stars](https://img.shields.io/github/stars/demaconsulting/SpdxTool?style=plastic)
![GitHub contributors](https://img.shields.io/github/contributors/demaconsulting/SpdxTool?style=plastic)
![GitHub](https://img.shields.io/github/license/demaconsulting/SpdxTool?style=plastic)
![Build](https://github.com/demaconsulting/SpdxTool/actions/workflows/build_on_push.yaml/badge.svg)

Dotnet tool for manipulating SPDX SBOM files


# Installation

The following will add SpdxTool to a Dotnet tool manifest file:

```
dotnet new tool-manifest # if you are setting up this repo
dotnet tool install --local DemaConsulting.SpdxTool
```

The tool can then be executed by:

```
dotnet spdx-tool <arguments>
```


# Usage

The following shows the command-line usage of SpdxTool:

```
Usage: spdx-tool [options] <command> [arguments]

Options:
  -h, --help                             Show this help message and exit
  -v, --version                          Show version information and exit

Commands:
  help <command>                         Display extended help about a command
  add-package                            Add package to SPDX document (workflow only).
  copy-package <arguments>               Copy package information from one SPDX document to another.
  query <pattern> <command> [arguments]  Query program output for value
  rename-id <arguments>                  Rename an element ID in an SPDX document.
  run-workflow <workflow.yaml>           Runs the workflow file
  sha256 <operation> <file>              Generate or verify sha256 hashes of files
  to-markdown <spdx.yaml> <out.md>       Create Markdown summary for SPDX document
```


# Workflow YAML Files

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

## YAML Variables

Variables are specified at the top of the workflow file in a parameters section:

```yaml
# Workflow parameters
parameters:
  parameter1: value1
  parameter2: value2
```

Variables can be expanded in step inputs using the dollar expansion syntax

```yaml
# Workflow steps
steps:
- command: <command-name>
  inputs:
    input1: ${{ parameter1 }}
    input2: Insert ${{ parameter2 }} in the middle
```

Variables can be overridden on the command line:

```
spdx-tool run-workflow workflow.yaml parameter1=command parameter2=line
```

Variables can be changed at runtime by some steps:

```yaml
# Workflow parameters
parameters:
  dotnet-version: unknown

steps:
- command: query
  inputs:
    output: dotnet-version
    pattern: '(?<value>\d+\.\d+\.\d+)'
    program: dotnet
    arguments:
    - '--version'
```


## YAML Commands

The following are the supported commands and their formats:

```yaml
steps:

  # Add a package to an SPDX document
- command: add-package
  inputs:
    package:
      id: <id>
      name: <name>
      copyright: <copyright>
      version: <version>
      download: <download-url>
      license: <license>       # optional
      purl: <package-url>      # optional
      cpe23: <cpe-identifier>  # optional
    spdx: <spdx.json>
    relationship: <relationship>
    element: <element>

  # Copy a package from one SPDX document to another SPDX document  
- command: copy-package
  inputs:
    from: <from.spdx.json>
    to: <to.spdx.json>
    package: <package>
    relationship: <relationship>
    element: <element>

  # Query information from the output of a program
- command: query
  inputs:
    output: <variable>
    pattern: <regex with 'value' capture>
    program: <program>
    arguments:
    - <argument>
    - <argument>

  # Rename the SPDX-ID of an element in an SPDX document
- command: rename-id
  inputs:
    spdx: <spdx.json>
    old: <old-id>
    new: <new-id>

  # Run a separate workflow file
- command: run-workflow
  inputs:
    file: other-workflow-file.yaml
    parameters:
      <optional parameters>

  # Perform Sha256 operations on the specified file
- command: help
  inputs:
    operation: generate | verify
    file: <file>

  # Create a summary markdown from the specified SPDX document
- command: to-markdown
  inputs:
    spdx: input.spdx.json
    markdown: output.md
```
