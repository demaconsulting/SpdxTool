---
title: Workflow files
summary: Write a YAML workflow that runs commands in order with parameters and variables.
aliases:
  - workflow
  - workflows
  - yaml
tags:
  - reference
order: 4
see-also:
  - commands
  - commands/run-workflow
  - msbuild
examples:
  - name: sample
    kind: source
    language: yaml
    save-as: workflow.yaml
    description: A workflow that updates a package, adds a build tool, checks the SBOM, and writes a summary.
    content: |
      # Decorate an SBOM: update a package, add a build tool, and write a summary.
      parameters:
        spdx: sbom.spdx.json
        version: 1.0.0

      steps:
        - displayName: Find the application package
          command: find-package
          inputs:
            output: app-id
            spdx: ${{ spdx }}
            name: Example Application

        - command: update-package
          inputs:
            spdx: ${{ spdx }}
            package:
              id: ${{ app-id }}
              version: ${{ version }}
              supplier: 'Organization: Example Corp'

        - command: add-package
          inputs:
            spdx: ${{ spdx }}
            package:
              id: SPDXRef-DotNet-SDK
              name: .NET SDK
              download: https://dotnet.microsoft.com/download
              version: 10.0.100
            relationships:
              - type: BUILD_TOOL_OF
                element: ${{ app-id }}

        - command: get-version
          inputs:
            output: app-version
            spdx: ${{ spdx }}
            id: ${{ app-id }}

        - command: set-variable
          inputs:
            value: ${{ app-id }} ${{ app-version }}
            output: summary

        - command: validate
          inputs:
            spdx: ${{ spdx }}

        - command: to-markdown
          inputs:
            spdx: ${{ spdx }}
            markdown: sbom.md
            title: Example Application SBOM

        - command: print
          inputs:
            text:
              - Decorated ${{ summary }}
  - name: run
    kind: command
    language: shell
    description: Run the sample workflow on sbom.spdx.json.
    content: '{{tool}} run-workflow workflow.yaml'
    expect: 'Decorated SPDXRef-Application 1.0.0'
  - name: parameter
    kind: command
    language: shell
    description: Override the declared parameter version.
    content: '{{tool}} run-workflow workflow.yaml version=2.0.0'
    expect: 'Decorated SPDXRef-Application 2.0.0'
  - name: outputs
    kind: command
    language: shell
    description: Also print the variables of the workflow when it ends.
    content: '{{tool}} run-workflow workflow.yaml --verbose'
    expect: 'app-version = 1.0.0'
  - name: undeclared
    kind: command
    language: shell
    description: A parameter that the workflow does not declare. Exits 1.
    content: '{{tool}} run-workflow workflow.yaml color=red'
    exit: 1
    expect: 'parameter color not defined'
---
A workflow file is YAML that runs commands of the tool one after the
other, so a build can decorate and check an SBOM in one call. Print the
complete sample with [[#sample]] and run it with the command that
[[#run]] prints.

**Structure.**

- `parameters`: optional; a map of variable names to default values.
- `steps`: required; the list of steps, run in order.
- `command`: in each step, the name of a command; see [[commands]].
- `displayName`: optional; text printed before the step runs.
- `inputs`: the inputs of the command, shown on its card under
  `{{tool}} help commands <command>`.

**Variables.** `${{ name }}` in a value is replaced by the variable
`name`, and `${{ environment.NAME }}` by the environment variable
`NAME`. Parameters are the first variables; steps such as
`find-package`, `get-version`, `query`, and `set-variable` add more
through their `output` input. A variable that is not defined stops the
workflow with an error.

**Parameters.** `name=value` after the file name on the command line
overrides a declared parameter, as in [[#parameter]]; an undeclared
name is an error, as in [[#undeclared]]. `--verbose` prints every
variable under `Outputs:` at the end, as in [[#outputs]].

**Errors.** The first step that fails stops the workflow, and the tool
exits 1. Steps that ran before it keep their changes to files.

**Nesting.** A `run-workflow` step runs another workflow from a file, a
URL, or a NuGet package, passes it `parameters`, and reads its variables
back through `outputs`; see [[commands/run-workflow]].
