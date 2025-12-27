# SPDX Tool Command Line

## Installation

SPDX Tool is distributed as a nuget package on [nuget.org](https://www.nuget.org/packages/DemaConsulting.SpdxTool).

The following will add SPDX Tool to a Dotnet tool manifest file:

```bash
dotnet new tool-manifest # if you are setting up this repo
dotnet tool install --local DemaConsulting.SpdxTool
```

The tool can then be executed by:

```bash
dotnet spdx-tool <arguments>
```

## Usage

The following usage information is printed by running the tool with no arguments, or requesting help information.

```text
DemaConsulting.SpdxTool 0.0.0

Usage: spdx-tool [options] <command> [arguments]

Options:
  -h, --help                               Show this help message and exit
  -v, --version                            Show version information and exit

Commands:
  help <command>                           Display extended help about a command
  add-package                              Add package to SPDX document (workflow only).
  add-relationship <spdx.json> <args>      Add relationship between elements.
  copy-package <spdx.json> <args>          Copy package between SPDX documents (workflow only).
  diagram <spdx.json> <mermaid.txt>        Generate mermaid diagram.
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

## Command Specific Usage

Querying for usage information on a specific command can be performed using the help command. For example:

```text
> dotnet spdx-tool help run-workflow

DemaConsulting.SpdxTool 0.1.0-beta.1

This command runs the steps specified in the workflow file/url.

From the command-line this can be used as:
  spdx-tool run-workflow <workflow.yaml> [parameter=value] [parameter=value]...

From a YAML file this can be used as:
  - command: run-workflow
    inputs:
      file: <workflow.yaml>         # Optional workflow file
      url: <url>                    # Optional workflow url
      integrity: <sha256>           # Optional workflow integrity check
      parameters:
        name: <value>               # Optional workflow parameter
        name: <value>               # Optional workflow parameter
      outputs:
        name: <variable>            # Optional output to save to variable
        name: <variable>            # Optional output to save to variable
```
