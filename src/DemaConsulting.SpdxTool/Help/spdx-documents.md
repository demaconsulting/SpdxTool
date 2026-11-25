---
title: SPDX documents
summary: What an SPDX document holds, and how the tool reads and changes it.
aliases:
  - spdx
  - sbom
  - documents
tags:
  - reference
order: 5
see-also:
  - commands/validate
  - commands
examples:
  - name: validate
    kind: command
    language: shell
    description: Check that the example document follows the SPDX rules.
    content: '{{tool}} validate sbom.spdx.json'
---
The tool reads SPDX 2.x documents in JSON, such as the software bill of
materials (SBOM) that `Microsoft.Sbom.Targets` writes into a NuGet
package as `_manifest/spdx_2.2/manifest.spdx.json`.

**Elements.** Every element has an ID in its `SPDXID` field, such as
`SPDXRef-Application`, and commands refer to elements by these IDs. The
ID `SPDXRef-DOCUMENT` is the document itself.

**Packages.** The `packages` list holds the components, each with a
`name`, a `versionInfo`, a `downloadLocation`, licenses, and optionally
a `supplier`, a file name, and external references such as a purl.

**Relationships.** The `relationships` list links two elements with a
type from the SPDX specification. The document `DESCRIBES` its root
packages; `CONTAINS`, `DEPENDS_ON`, and `DEPENDENCY_OF` relate packages,
and `BUILD_TOOL_OF` marks a tool. Each entry has `spdxElementId`,
`relationshipType`, and `relatedSpdxElement`.

**The examples.** The help examples use `sbom.spdx.json`, the SBOM of an
example application: the root package `SPDXRef-Application` (version
1.0.0) and the library `SPDXRef-Library` (version 2.1.0) that it depends
on. A second document, `lib.spdx.json`, holds the package
`SPDXRef-Json`. The command that [[#validate]] prints checks the first.

**Changes.** Commands that change a document read it, change it in
memory, and write it back to the same file. Each write adds the creator
`Tool: DemaConsulting.SpdxTool-<version>` once, so the document records
that the tool edited it. Write to a copy when the original must stay.

**Checks.** `validate` checks the SPDX rules, and with `ntia` also the
NTIA minimum elements of an SBOM; see [[commands/validate]].
