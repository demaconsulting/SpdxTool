---
title: Decorating SBOMs in dotnet pack
summary: Decorate the SBOM of a NuGet package with a workflow during dotnet pack.
aliases:
  - targets
  - pack
  - dotnet-pack
tags:
  - tutorial
order: 6
see-also:
  - workflow-files
  - getting-started
examples:
  - name: add-packages
    kind: snippet
    language: shell
    save-as: add-packages.sh
    description: Reference the SBOM generator and the SpdxTool targets from the project to pack.
    content: |
      dotnet add package Microsoft.Sbom.Targets
      dotnet add package DemaConsulting.SpdxTool.Targets
  - name: properties
    kind: snippet
    language: xml
    save-as: sbom.props
    description: Turn on SBOM generation and decoration in the project file.
    content: |
      <PropertyGroup>
        <GenerateSBOM>true</GenerateSBOM>
        <DecorateSBOM>true</DecorateSBOM>
      </PropertyGroup>
  - name: workflow
    kind: source
    language: yaml
    save-as: spdx-workflow.yaml
    description: A decoration workflow that records the .NET SDK as the build tool of the package.
    content: |
      # Decorate the SBOM that Microsoft.Sbom.Targets wrote into the package.
      parameters:
        spdx: _manifest/spdx_2.2/manifest.spdx.json
        name: Example Application

      steps:
        - command: find-package
          inputs:
            output: root-id
            spdx: ${{ spdx }}
            name: ${{ name }}

        - command: query
          inputs:
            output: dotnet-version
            pattern: '(?<value>\d+\.\d+\.\d+)'
            program: dotnet
            arguments:
              - --version

        - command: add-package
          inputs:
            spdx: ${{ spdx }}
            package:
              id: SPDXRef-DotNet-SDK
              name: .NET SDK
              download: https://dotnet.microsoft.com/download
              version: ${{ dotnet-version }}
            relationships:
              - type: BUILD_TOOL_OF
                element: ${{ root-id }}

        - command: validate
          inputs:
            spdx: ${{ spdx }}

        - command: print
          inputs:
            text:
              - Added the .NET SDK to ${{ root-id }}
  - name: try
    kind: command
    language: shell
    description: Try the workflow on a local SBOM before packing.
    content: '{{tool}} run-workflow spdx-workflow.yaml spdx=sbom.spdx.json'
    expect: 'Added the .NET SDK to SPDXRef-Application'
---
The package `DemaConsulting.SpdxTool.Targets` runs a workflow of this
tool on the SBOM that `Microsoft.Sbom.Targets` adds to a NuGet package,
so every `dotnet pack` produces a decorated, checked SBOM.

1. **Install the tool** as a local tool of the repository; see
   [[getting-started]]. The targets run it as `dotnet {{tool}}`.
2. **Reference the packages** with the commands that [[#add-packages]]
   prints, in the project to pack.
3. **Turn it on** with the properties that [[#properties]] prints. The
   targets run only when `IsPackable`, `GenerateSBOM`, and
   `DecorateSBOM` are all `true`.
4. **Write the workflow** as `spdx-workflow.yaml` beside the project
   file. [[#workflow]] prints one that records the .NET SDK as a build
   tool; set its `name` to the name of the package in the SBOM, usually
   the package ID. A missing workflow file fails the build.
5. **Try it** on a local SBOM with the command that [[#try]] prints,
   then run `dotnet pack`.

**How it runs.** After the SBOM is generated, the targets unzip the
`.nupkg`, run `<SpdxToolCommand> run-workflow "<SpdxWorkflowFile>"` in
the unzipped folder, and zip it again. Paths in the workflow are
relative to the package root, so the SBOM is
`_manifest/spdx_2.2/manifest.spdx.json`. A project with several target
frameworks is decorated once.

| Property | Default |
| --- | --- |
| `DecorateSBOM` | `false` |
| `SpdxWorkflowFile` | `$(MSBuildProjectDirectory)/spdx-workflow.yaml` |
| `SpdxToolCommand` | `dotnet spdx-tool` |
