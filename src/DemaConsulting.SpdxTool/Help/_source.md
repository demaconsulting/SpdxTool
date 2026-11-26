---
name: spdx-tool
schema: 2
title: SpdxTool
summary: Built-in help of SpdxTool, which checks and edits SPDX SBOM documents.
---
{{tool}} reads, checks, and edits SPDX 2.x JSON documents (SBOMs) with commands, and runs workflow files that
chain those commands.

Run a command as `{{tool}} [options] <command> [arguments]`. The `{{tool}} help` command must be the first
argument and takes topic names, such as `{{tool}} help getting-started`, or command names, such as
`{{tool}} help validate`.
