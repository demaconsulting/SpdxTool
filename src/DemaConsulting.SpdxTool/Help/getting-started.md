---
title: Getting started
summary: Install the tool, check that it works, and check and summarize a first SBOM.
aliases:
  - start
  - quickstart
  - examples
  - install
tags:
  - tutorial
order: 1
see-also:
  - commands
  - spdx-documents
  - workflow-files
examples:
  - name: install-local
    kind: snippet
    language: shell
    save-as: install-local.sh
    description: Add the tool to the tool manifest of a repository.
    content: |
      dotnet new tool-manifest
      dotnet tool install --local DemaConsulting.SpdxTool
  - name: ci-steps
    kind: snippet
    language: yaml
    save-as: ci-steps.yaml
    description: GitHub Actions steps that set up .NET and run a workflow file with the tool.
    content: |
      - name: Check out the repository
        uses: actions/checkout@v4
      - name: Set up .NET 10
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.x
      - name: Restore the local .NET tools
        run: dotnet tool restore
      - name: Decorate and check the SBOM
        run: dotnet {{tool}} run-workflow workflow.yaml
  - name: version
    kind: command
    language: shell
    description: Print the version and exit.
    content: '{{tool}} --version'
  - name: validate
    kind: command
    language: shell
    description: Check an SPDX document against the SPDX rules. Exits 0 when it has no issues.
    content: '{{tool}} validate sbom.spdx.json'
  - name: summary
    kind: command
    language: shell
    description: Write a Markdown summary of the document to sbom.md.
    content: '{{tool}} to-markdown sbom.spdx.json sbom.md'
  - name: topic
    kind: command
    language: shell
    description: Print the help card of a command, here validate.
    content: '{{tool}} help validate'
    expect: 'validate <spdx.json> [ntia]'
---
{{tool}} {{version}} is a .NET tool.
It checks and edits SPDX documents, the software bills of materials (SBOMs)
that build tools write. The examples read `sbom.spdx.json`, a small SBOM
of an example application; see [[spdx-documents]].

1. **Install it.** The tool needs the .NET 10 SDK or later. For a
   repository or CI, install it as a local tool. Run the commands
   that [[#install-local]] prints once, without
   `dotnet new tool-manifest` when `.config/dotnet-tools.json` exists, and
   commit that file. In GitHub Actions, copy the steps that
   [[#ci-steps]] prints. They set up .NET 10 with
   `actions/setup-dotnet`, run `dotnet tool restore`, and then run
   `dotnet {{tool}} run-workflow` on a workflow file such as the sample
   in [[workflow-files]]. For one machine, install it globally and run it
   as `{{tool}}`:

   ```bash
   dotnet tool install -g DemaConsulting.SpdxTool
   ```

2. **Check it.** The command that [[#version]] prints shows the version.
   `{{tool}} --validate` runs the built-in self-tests; see [[self-validation]].
3. **Check an SBOM.** The command that [[#validate]] prints checks the
   document and exits 1 when it finds issues.
4. **Summarize it.** The command that [[#summary]] prints writes a Markdown
   summary. Every command is listed in [[commands]].
5. **Automate it.** A workflow file runs several commands in order; see
   [[workflow-files]]. To decorate the SBOM of a NuGet package during
   `dotnet pack`, see [[msbuild]].

Print any help topic with `{{tool}} help <topic>`, and the card of a
command with `{{tool}} help <command>`, as in the command that
[[#topic]] prints.
Add `--search <words>` to find a topic, and `--json` to get any help answer as JSON.
