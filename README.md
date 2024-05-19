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
  run-workflow <workflow.yaml>           Runs the workflow file
  to-markdown <spdx.yaml> <out.md>       Create Markdown summary for SPDX document
```


# Workflow YAML Files

The SpdxTool can be driven using workflow yaml files of the following format:

```yaml
steps:
- command: <command-name>
  <arguments mapping>

- command: <command-name>
  <arguments mapping>
```

## YAML Commands

The following are the supported commands and their formats:

```yaml
steps:

  # Run a separate workflow file
- command: run-workflow
  file: other-workflow-file.yaml

  # Create a summary markdown from the specified SPDX document
- command: to-markdown
  spdx: input.spdx.json
  markdown: output.md
```
