# SPDX Tool Command Line

## Installation

SPDX Tool is distributed as a nuget package on [nuget.org][nuget-package].

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
       spdx-tool help [topic]

Options:
  -h, --help                               Show this help message and exit
  -v, --version                            Show version information and exit
  -l, --log <log-file>                     Log output to file
  -s, --silent                             Silence console output
      --validate                           Perform self-validation
  -r, --result <file>                      Self-validation result file (.trx TRX or .xml JUnit XML)

Commands:
  help                                     Display extended help about a command (workflow only).
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

Run 'spdx-tool help' for topics and examples.
```

## Command Specific Usage

The built-in help prints the help topics with `dotnet spdx-tool help`, and the card of a specific
command with `dotnet spdx-tool help <command>`, which is the same as
`dotnet spdx-tool help commands <command>`. For example:

```bash
dotnet spdx-tool help run-workflow
```

The card shows the command-line and workflow-step usage of the command, notes on its behavior, and
runnable examples. `help` must be the first argument.

[nuget-package]: https://www.nuget.org/packages/DemaConsulting.SpdxTool
