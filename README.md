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
  -h, --help                               Show this help message and exit
  -v, --version                            Show version information and exit

Commands:
  help <command>                           Display extended help about a command
  add-package                              Add package to SPDX document (workflow only).
  add-relationship <spdx.json> <args>      Add relationship between elements.
  copy-package <spdx.json> <args>          Copy package between SPDX documents (workflow only).
  find-package <spdx.json> <criteria>      Find package ID in SPDX document
  print <text>                             Print text to the console
  query <pattern> <program> [args]         Query program output for value
  rename-id <arguments>                    Rename an element ID in an SPDX document.
  run-workflow <workflow.yaml>             Runs the workflow file
  sha256 <operation> <file>                Generate or verify sha256 hashes of files
  to-markdown <spdx.json> <out.md> [args]  Create Markdown summary for SPDX document
  update-package                           Update package in SPDX document (workflow only).
  validate <spdx.json> [ntia]              Validate SPDX document for issues
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
    spdx: <spdx.json>             # SPDX file name
    package:                      # New package information
      id: <id>                    # New package ID
      name: <name>                # New package name
      download: <download-url>    # New package download URL
      version: <version>          # Optional package version
      filename: <filename>        # Optional package filename
      supplier: <supplier>        # Optional package supplier
      originator: <originator>    # Optional package originator
      homepage: <homepage>        # Optional package homepage
      copyright: <copyright>      # Optional package copyright
      summary: <summary>          # Optional package summary
      description: <description>  # Optional package description
      license: <license>          # Optional package license
      purl: <package-url>         # Optional package purl
      cpe23: <cpe-identifier>     # Optional package cpe23
    relationships:                # Relationships
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment

  # Add a relationship to an SPDX document
- command: add-relationship
  inputs:
    spdx: <spdx.json>             # SPDX file name
    id: <id>                      # Element ID
    relationships:
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment

  # Copy a package from one SPDX document to another SPDX document  
- command: copy-package
  inputs:
    from: <from.spdx.json>        # Source SPDX file name
    to: <to.spdx.json>            # Destination SPDX file name
    package: <package>            # Package ID
    recursive: true               # Optional recursive flag
    relationships:                # Relationships
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment
    - type: <relationship>        # Relationship type
      element: <element>          # Related element
      comment: <comment>          # Optional comment

  # finds the package ID for a package in an SPDX document
- command: find-package
  inputs:
    output: <variable>            # Output variable for package ID
    spdx: <spdx.json>             # SPDX file name
    name: <name>                  # Optional package name
    version: <version>            # Optional package version
    filename: <filename>          # Optional package filename
    download: <url>               # Optional package download URL

  # Print text to the console
- command: print
  inputs:
    text:
    - Some text to print
    - The value of variable is ${{ variable }}

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
    spdx: <spdx.json>             # SPDX file name
    old: <old-id>                 # Old element ID
    new: <new-id>                 # New element ID

  # Run a separate workflow file
- command: run-workflow
  inputs:
    file: other-workflow-file.yaml
    parameters:
      <optional parameters>

  # Perform Sha256 operations on the specified file
- command: sha256
  inputs:
    operation: generate | verify
    file: <file>

  # Create a summary markdown from the specified SPDX document
- command: to-markdown
  inputs:
    spdx: <spdx.json>             # SPDX file name
    markdown: <out.md>            # Output markdown file
    title: <title>                # Optional title
    depth: <depth>                # Optional heading depth

  # Update a package in an SPDX document
- command: update-package
  inputs:
    spdx: <spdx.json>             # SPDX filename
    package:                      # Package information
      id: <id>                    # Package ID
      name: <name>                # Optional new package name
      download: <download-url>    # Optional new package download URL
      version: <version>          # Optional new package version
      filename: <filename>        # Optional new package filename
      supplier: <supplier>        # Optional new package supplier
      originator: <originator>    # Optional new package originator
      homepage: <homepage>        # Optional new package homepage
      copyright: <copyright>      # Optional new package copyright
      summary: <summary>          # Optional new package summary
      description: <description>  # Optional new package description
      license: <license>          # Optional new package license

  # Validate an SPDX document
- command: validate
  inputs:
    spdx: <spdx.json>             # SPDX file name
    ntia: true                    # Optional NTIA checking
```
